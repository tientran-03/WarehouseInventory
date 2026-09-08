using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class StocktakeRepository : IStocktakeRepository
{
    private readonly AppDbContext _context;

    public StocktakeRepository(AppDbContext context) => _context = context;

    public async Task<List<Stocktake>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.Stocktakes
            .Include(s => s.Warehouse)
            .Where(s => s.TenantId == tenantId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<Stocktake?> GetByIdAsync(Guid id)
    {
        return await _context.Stocktakes
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(Stocktake stocktake) => await _context.Stocktakes.AddAsync(stocktake);

    public Task UpdateAsync(Stocktake stocktake)
    {
        _context.Stocktakes.Update(stocktake);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
