using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface ITenantRepository
{
    Task<IEnumerable<Tenant>> GetAllActiveAsync();
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<bool> AnyCodeExistsAsync(string code);
    Task<bool> AnyCodeExistsForOtherAsync(string code, Guid id);
    Task AddAsync(Tenant tenant);
    Task UpdateAsync(Tenant tenant);
    Task SaveChangesAsync();
}
