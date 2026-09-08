using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class WarehouseZoneRepository : IWarehouseZoneRepository
{
    private readonly AppDbContext _context;

    public WarehouseZoneRepository(AppDbContext context) => _context = context;

    public async Task<List<WarehouseZone>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.WarehouseZones
            .Include(z => z.Warehouse)
            .Where(z => z.TenantId == tenantId)
            .OrderBy(z => z.Warehouse!.Name)
            .ThenBy(z => z.Code)
            .ToListAsync();
    }

    public async Task<WarehouseZone?> GetByIdAsync(Guid id)
    {
        return await _context.WarehouseZones
            .Include(z => z.Warehouse)
            .FirstOrDefaultAsync(z => z.Id == id);
    }

    public Task<bool> AnyCodeExistsInWarehouseAsync(Guid warehouseId, string code)
    {
        return _context.WarehouseZones.AnyAsync(z =>
            z.WarehouseId == warehouseId &&
            z.Code == code);
    }

    public async Task AddAsync(WarehouseZone zone) => await _context.WarehouseZones.AddAsync(zone);

    public Task UpdateAsync(WarehouseZone zone)
    {
        _context.WarehouseZones.Update(zone);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(WarehouseZone zone)
    {
        _context.WarehouseZones.Remove(zone);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
