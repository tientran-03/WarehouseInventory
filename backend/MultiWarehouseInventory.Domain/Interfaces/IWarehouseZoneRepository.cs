using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IWarehouseZoneRepository
{
    Task<List<WarehouseZone>> GetByTenantAsync(Guid tenantId);
    Task<WarehouseZone?> GetByIdAsync(Guid id);
    Task<bool> AnyCodeExistsInWarehouseAsync(Guid warehouseId, string code);
    Task AddAsync(WarehouseZone zone);
    Task UpdateAsync(WarehouseZone zone);
    Task DeleteAsync(WarehouseZone zone);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
