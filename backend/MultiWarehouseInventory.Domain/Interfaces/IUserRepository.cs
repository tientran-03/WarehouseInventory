using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash);
    Task<bool> AnyUsernameExistsAsync(string username);
    Task<bool> AnyEmailExistsAsync(string email);
    Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId);
    Task<IReadOnlyList<User>> GetAllCompanyAccountsAsync();
    Task AddAsync(User user);
    Task SaveChangesAsync();
}
