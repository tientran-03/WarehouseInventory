using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IWarehouseInventoryRepository
{
    Task<WarehouseInventory?> GetByIdAsync(Guid id);

    Task<List<WarehouseInventory>> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId, Guid tenantId);

    Task<WarehouseInventory?> GetByWarehouseProductAndZoneAsync(
        Guid warehouseId,
        Guid productId,
        Guid tenantId,
        Guid? warehouseZoneId);

    Task<int> GetOnHandStockByZoneAsync(Guid warehouseZoneId);

    Task<bool> HasInventoryInZoneAsync(Guid warehouseZoneId);

    Task<List<WarehouseInventory>> GetByTenantAsync(Guid tenantId);

    Task<List<WarehouseInventory>> GetByWarehouseAsync(Guid warehouseId);

    Task<List<WarehouseInventory>> GetByWarehousesAndProductsAsync(
        Guid tenantId,
        IEnumerable<Guid> warehouseIds,
        IEnumerable<Guid> productIds);

    Task AddAsync(WarehouseInventory inventory);

    Task UpdateAsync(WarehouseInventory inventory);
    Task UpdateRangeAsync(IEnumerable<WarehouseInventory> inventories);
    Task DeleteAsync(WarehouseInventory inventory);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
