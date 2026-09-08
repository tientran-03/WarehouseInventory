using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class WarehouseInventoryRepository : Repository<WarehouseInventory>, IWarehouseInventoryRepository
{
    private static readonly Func<IQueryable<WarehouseInventory>, IQueryable<WarehouseInventory>> WithDetails =
        q => q.Include(i => i.Warehouse).Include(i => i.Product).Include(i => i.WarehouseZone);

    public WarehouseInventoryRepository(AppDbContext context) : base(context) { }

    public async Task<WarehouseInventory?> GetByIdAsync(Guid id)
    {
        return await WithDetails(_context.WarehouseInventories)
            .FirstOrDefaultAsync(wi => wi.Id == id);
    }

    public async Task<List<WarehouseInventory>> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId, Guid tenantId)
    {
        return await WithDetails(_context.WarehouseInventories)
            .Where(wi =>
                wi.WarehouseId == warehouseId &&
                wi.ProductId == productId &&
                wi.TenantId == tenantId)
            .OrderBy(wi => wi.WarehouseZone!.Code)
            .ToListAsync();
    }

    public async Task<WarehouseInventory?> GetByWarehouseProductAndZoneAsync(
        Guid warehouseId,
        Guid productId,
        Guid tenantId,
        Guid? warehouseZoneId)
    {
        return await WithDetails(_context.WarehouseInventories)
            .FirstOrDefaultAsync(wi =>
                wi.WarehouseId == warehouseId &&
                wi.ProductId == productId &&
                wi.TenantId == tenantId &&
                wi.WarehouseZoneId == warehouseZoneId);
    }

    public async Task<int> GetOnHandStockByZoneAsync(Guid warehouseZoneId)
    {
        return await _context.WarehouseInventories
            .Where(wi => wi.WarehouseZoneId == warehouseZoneId)
            .SumAsync(wi => (int?)wi.OnHandStock) ?? 0;
    }

    public async Task<bool> HasInventoryInZoneAsync(Guid warehouseZoneId)
    {
        try
        {
            return await _context.WarehouseInventories.AnyAsync(wi => wi.WarehouseZoneId == warehouseZoneId);
        }
        catch (Exception)
        {
            // Nếu có lỗi, giả định là có tồn kho để an toàn
            return true;
        }
    }

    public async Task<List<WarehouseInventory>> GetByTenantAsync(Guid tenantId)
    {
        return await WithDetails(_context.WarehouseInventories)
            .Where(wi => wi.TenantId == tenantId)
            .OrderBy(wi => wi.Warehouse!.Name)
            .ThenBy(wi => wi.Product!.Sku)
            .ToListAsync();
    }

    public async Task<List<WarehouseInventory>> GetByWarehouseAsync(Guid warehouseId)
    {
        return await WithDetails(_context.WarehouseInventories)
            .Where(wi => wi.WarehouseId == warehouseId)
            .OrderBy(wi => wi.Product!.Sku)
            .ToListAsync();
    }

    public async Task<List<WarehouseInventory>> GetByWarehousesAndProductsAsync(
        Guid tenantId,
        IEnumerable<Guid> warehouseIds,
        IEnumerable<Guid> productIds)
    {
        var warehouseIdList = warehouseIds.Distinct().ToList();
        var productIdList = productIds.Distinct().ToList();

        return await _context.WarehouseInventories
            .Where(wi =>
                wi.TenantId == tenantId &&
                warehouseIdList.Contains(wi.WarehouseId) &&
                productIdList.Contains(wi.ProductId))
            .ToListAsync();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
