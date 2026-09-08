using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure.Data;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Tenant>> GetAllActiveAsync()
    {
        return await _context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();
    }

    public new async Task<Tenant?> GetByIdAsync(Guid id)
    {
        return await _context.Tenants.FindAsync(id);
    }

    public async Task<bool> AnyCodeExistsAsync(string code)
    {
        return await _context.Tenants.AnyAsync(t => t.Code == code);
    }

    public async Task<bool> AnyCodeExistsForOtherAsync(string code, Guid id)
    {
        return await _context.Tenants.AnyAsync(t => t.Code == code && t.Id != id);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
