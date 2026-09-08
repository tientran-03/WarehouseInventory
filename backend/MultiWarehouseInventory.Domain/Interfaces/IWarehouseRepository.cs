using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IWarehouseRepository
{
    Task<IEnumerable<Warehouse>> GetAllActiveAsync();
    Task<List<Warehouse>> GetActiveWarehousesByTenantAsync(Guid tenantId);
    Task<Warehouse?> GetByIdAsync(Guid id);
    Task<bool> AnyCodeExistsAsync(string code);
    Task<bool> AnyCodeExistsForOtherAsync(string code, Guid id);
    Task AddAsync(Warehouse warehouse);
    Task UpdateAsync(Warehouse warehouse);
    Task SaveChangesAsync();
}
