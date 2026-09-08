using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context) { }

    public new async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products.Where(p => p.IsActive).ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.Products
            .Where(p => p.TenantId == tenantId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public new async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product?> GetBySkuAsync(string sku, Guid tenantId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(p => p.Sku == sku && p.TenantId == tenantId);
    }

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
