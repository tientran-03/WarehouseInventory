

namespace MultiWarehouseInventory.Application.DTOs
{
    public record UpsertTenantRequest(
         string Name,
         string Code,
         string? AccountUsername = null,
         string? AccountPassword = null,
         string? AccountEmail = null
     );

    public record CreateCompanyAccountRequest(
        string? Username,
        string Password,
        string Email
    );

    public record ResendVerificationEmailRequest(string Email);

    public record CompanyAccountResponse(
        string Username,
        string Email,
        bool IsEmailVerified,
        bool MustChangePassword
    );

    public record TenantResponse(
        Guid Id,
        string Name,
        string Code,
        bool IsActive,
        IReadOnlyList<CompanyAccountResponse> Accounts
    );
}
