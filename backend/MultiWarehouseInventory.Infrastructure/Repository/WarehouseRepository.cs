using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure.Data;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class WarehouseRepository : Repository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Warehouse>> GetAllActiveAsync()
    {
        return await _context.Warehouses
            .Where(w => w.IsActive)
            .ToListAsync();
    }

    public async Task<List<Warehouse>> GetActiveWarehousesByTenantAsync(Guid tenantId)
    {
        return await _context.Warehouses
            .Where(w => w.IsActive && w.TenantId == tenantId)
            .ToListAsync();
    }

    public new async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Warehouses.FindAsync(id);
    }

    public async Task<bool> AnyCodeExistsAsync(string code)
    {
        return await _context.Warehouses.AnyAsync(w => w.Code == code);
    }

    public async Task<bool> AnyCodeExistsForOtherAsync(string code, Guid id)
    {
        return await _context.Warehouses.AnyAsync(w => w.Code == code && w.Id != id);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
