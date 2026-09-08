using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Enums;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;
    private readonly IAccountSecurityService _accountSecurityService;
    private readonly ICompanyAccountEmailSender _companyAccountEmailSender;

    public TenantService(
        ITenantRepository tenantRepository,
        IUserRepository userRepository,
        IMapper mapper,
        IAccountSecurityService accountSecurityService,
        ICompanyAccountEmailSender companyAccountEmailSender)
    {
        _tenantRepository = tenantRepository;
        _userRepository = userRepository;
        _mapper = mapper;
        _accountSecurityService = accountSecurityService;
        _companyAccountEmailSender = companyAccountEmailSender;
    }

    public async Task<IEnumerable<TenantResponse>> GetAllAsync()
    {
        var tenants = await _tenantRepository.GetAllActiveAsync();
        var accounts = await _userRepository.GetAllCompanyAccountsAsync();
        return tenants.Select(t => MapResponse(t, accounts.Where(u => u.TenantId == t.Id).ToList()));
    }

    public async Task<TenantResponse> GetByIdAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), id);
        }

        return MapResponse(tenant, await _userRepository.GetByTenantIdAsync(id));
    }

    public async Task<TenantResponse> CreateAsync(UpsertTenantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountEmail) || string.IsNullOrWhiteSpace(request.AccountPassword))
        {
            throw new BadRequestException("Cần nhập email và mật khẩu tạm thời khi thêm công ty.");
        }

        if (await _tenantRepository.AnyCodeExistsAsync(request.Code))
        {
            throw new BadRequestException($"Mã công ty '{request.Code}' đã tồn tại trong hệ thống.");
        }

        var accountEmail = NormalizeEmail(request.AccountEmail);
        var accountUsername = string.IsNullOrWhiteSpace(request.AccountUsername)
            ? accountEmail
            : request.AccountUsername.Trim();
        ValidatePassword(request.AccountPassword);

        if (await _userRepository.AnyUsernameExistsAsync(accountUsername))
        {
            throw new BadRequestException($"Tên đăng nhập '{accountUsername}' đã tồn tại.");
        }

        if (await _userRepository.AnyEmailExistsAsync(accountEmail))
        {
            throw new BadRequestException($"Email '{accountEmail}' đã được sử dụng.");
        }

        EnsureEmailDeliveryIsConfigured();

        var tenant = _mapper.Map<Tenant>(request);
        tenant.IsActive = true;
        await _tenantRepository.AddAsync(tenant);

        var account = await CreateCompanyAccountAsync(tenant, accountUsername, accountEmail, request.AccountPassword);
        await _userRepository.SaveChangesAsync();
        return MapResponse(tenant, [account]);
    }

    public async Task AddCompanyAccountAsync(Guid tenantId, CreateCompanyAccountRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }

        var email = NormalizeEmail(request.Email);
        var username = string.IsNullOrWhiteSpace(request.Username) ? email : request.Username.Trim();
        ValidatePassword(request.Password);

        if (await _userRepository.AnyUsernameExistsAsync(username))
        {
            throw new BadRequestException($"Tên đăng nhập '{username}' đã tồn tại.");
        }

        if (await _userRepository.AnyEmailExistsAsync(email))
        {
            throw new BadRequestException($"Email '{email}' đã được sử dụng.");
        }

        EnsureEmailDeliveryIsConfigured();
        await CreateCompanyAccountAsync(tenant, username, email, request.Password);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ResendVerificationEmailAsync(Guid tenantId, ResendVerificationEmailRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var account = (await _userRepository.GetByTenantIdAsync(tenantId))
            .FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));

        if (account is null)
        {
            throw new NotFoundException(nameof(User), tenantId);
        }

        if (account.EmailVerifiedAt.HasValue)
        {
            throw new BadRequestException("Email của tài khoản này đã được xác minh.");
        }

        EnsureEmailDeliveryIsConfigured();

        var verificationToken = _accountSecurityService.GenerateSecureToken();
        account.EmailVerificationTokenHash = _accountSecurityService.HashToken(verificationToken);
        account.EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24);
        await _companyAccountEmailSender.SendVerificationEmailAsync(account.Email, account.Username, verificationToken);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid id, UpsertTenantRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), id);
        }

        if (await _tenantRepository.AnyCodeExistsForOtherAsync(request.Code, id))
        {
            throw new BadRequestException($"Mã công ty '{request.Code}' đã được sử dụng bởi công ty khác.");
        }

        tenant.Name = request.Name;
        tenant.Code = request.Code;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _tenantRepository.UpdateAsync(tenant);
        await _tenantRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetByIdAsync(id);
        if (tenant is null)
        {
            throw new NotFoundException(nameof(Tenant), id);
        }

        tenant.IsActive = false;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _tenantRepository.UpdateAsync(tenant);
        await _tenantRepository.SaveChangesAsync();
    }

    private async Task<User> CreateCompanyAccountAsync(Tenant tenant, string username, string email, string password)
    {
        var verificationToken = _accountSecurityService.GenerateSecureToken();
        var account = new User
        {
            Username = username,
            Email = email,
            PasswordHash = _accountSecurityService.HashPassword(password),
            EmailVerificationTokenHash = _accountSecurityService.HashToken(verificationToken),
            EmailVerificationTokenExpiresAt = DateTime.UtcNow.AddHours(24),
            MustChangePassword = true,
            Role = UserRole.Manager,
            TenantId = tenant.Id,
            IsActive = true,
        };

        await _userRepository.AddAsync(account);
        await _companyAccountEmailSender.SendVerificationEmailAsync(email, username, verificationToken);
        return account;
    }

    private void EnsureEmailDeliveryIsConfigured()
    {
        if (!_companyAccountEmailSender.IsConfigured)
        {
            throw new BadRequestException("Dịch vụ email chưa được cấu hình. Không thể gửi email xác minh cho công ty.");
        }
    }

    private static string NormalizeEmail(string? email)
    {
        var normalized = email?.Trim().ToLowerInvariant() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 254 || !normalized.Contains('@', StringComparison.Ordinal))
        {
            throw new BadRequestException("Email không hợp lệ.");
        }

        return normalized;
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
        {
            throw new BadRequestException("Mật khẩu tạm thời cần tối thiểu 8 ký tự.");
        }
    }

    private static TenantResponse MapResponse(Tenant tenant, IReadOnlyList<User> accounts) =>
        new(
            tenant.Id,
            tenant.Name,
            tenant.Code,
            tenant.IsActive,
            accounts.Select(account => new CompanyAccountResponse(
                account.Username,
                account.Email,
                account.EmailVerifiedAt.HasValue,
                account.MustChangePassword)).ToList());
}
