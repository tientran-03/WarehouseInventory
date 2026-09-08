namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IAccountSecurityService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
    bool NeedsRehash(string passwordHash);
    string GenerateSecureToken();
    string HashToken(string token);
}
