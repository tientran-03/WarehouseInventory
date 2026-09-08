using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class WarehouseTransferRepository : IWarehouseTransferRepository
{
    private readonly AppDbContext _context;

    public WarehouseTransferRepository(AppDbContext context) => _context = context;

    public async Task<List<WarehouseTransfer>> GetByTenantAsync(Guid tenantId)
    {
        return await _context.WarehouseTransfers
            .Include(t => t.Product)
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .Where(t => t.TenantId == tenantId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<WarehouseTransfer?> GetByIdAsync(Guid id)
    {
        return await _context.WarehouseTransfers
            .Include(t => t.Product)
            .Include(t => t.FromWarehouse)
            .Include(t => t.ToWarehouse)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task AddAsync(WarehouseTransfer transfer) => await _context.WarehouseTransfers.AddAsync(transfer);

    public Task UpdateAsync(WarehouseTransfer transfer)
    {
        _context.WarehouseTransfers.Update(transfer);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
