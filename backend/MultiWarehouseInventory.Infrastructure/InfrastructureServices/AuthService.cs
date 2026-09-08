using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task VerifyEmailAsync(string token);
    Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IAccountSecurityService _accountSecurityService;

    public AuthService(
        AppDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IAccountSecurityService accountSecurityService)
    {
        _context = context;
        _jwtTokenGenerator = jwtTokenGenerator;
        _accountSecurityService = accountSecurityService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var identifier = request.UserName.Trim();
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);
        if (user == null || !user.IsActive || !_accountSecurityService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        if (user.TenantId.HasValue && !user.EmailVerifiedAt.HasValue)
        {
            throw new UnauthorizedException("Email chưa được xác minh. Vui lòng kiểm tra hộp thư và xác minh tài khoản trước khi đăng nhập.");
        }

        if (_accountSecurityService.NeedsRehash(user.PasswordHash))
        {
            user.PasswordHash = _accountSecurityService.HashPassword(request.Password);
            await _context.SaveChangesAsync();
        }

        return CreateAuthResponse(user);
    }

    public async Task VerifyEmailAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new BadRequestException("Liên kết xác minh không hợp lệ.");
        }

        var tokenHash = _accountSecurityService.HashToken(token);
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash);
        if (user is null || !user.IsActive || user.EmailVerifiedAt.HasValue)
        {
            throw new BadRequestException("Liên kết xác minh không hợp lệ hoặc đã được sử dụng.");
        }

        if (!user.EmailVerificationTokenExpiresAt.HasValue || user.EmailVerificationTokenExpiresAt.Value < DateTime.UtcNow)
        {
            throw new BadRequestException("Liên kết xác minh đã hết hạn. Hãy liên hệ quản trị viên để gửi lại email xác minh.");
        }

        user.EmailVerifiedAt = DateTime.UtcNow;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;
        await _context.SaveChangesAsync();
    }

    public async Task<AuthResponse> ChangePasswordAsync(Guid userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 8)
        {
            throw new BadRequestException("Mật khẩu mới cần tối thiểu 8 ký tự.");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null || !user.IsActive)
        {
            throw new UnauthorizedException("Tài khoản không còn hoạt động.");
        }

        if (!_accountSecurityService.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            throw new BadRequestException("Mật khẩu hiện tại không chính xác.");
        }

        if (_accountSecurityService.VerifyPassword(request.NewPassword, user.PasswordHash))
        {
            throw new BadRequestException("Mật khẩu mới phải khác mật khẩu hiện tại.");
        }

        user.PasswordHash = _accountSecurityService.HashPassword(request.NewPassword);
        user.MustChangePassword = false;
        await _context.SaveChangesAsync();

        return CreateAuthResponse(user);
    }

    private AuthResponse CreateAuthResponse(MultiWarehouseInventory.Domain.Entities.User user) =>
        new(
            _jwtTokenGenerator.GenerateToken(user),
            user.Username,
            DateTime.UtcNow.AddHours(2),
            user.Role.ToString(),
            user.TenantId,
            user.MustChangePassword);
}
