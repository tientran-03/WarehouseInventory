using System.Text.Json.Serialization;

namespace MultiWarehouseInventory.Domain.Common;

public class LoginRequest
{
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

public record AuthResponse(
    string Token,
    string Username,
    DateTime Expiration,
    string Role,
    Guid? TenantId,
    bool RequiresPasswordChange);

public class ChangePasswordRequest
{
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = string.Empty;

    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = string.Empty;
}
