using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context) { }

    public Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        return Task.CompletedTask;
    }

    public async Task<Order?> GetByOrderCodeAsync(Guid tenantId, string orderCode)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.OrderCode == orderCode);
    }

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Include(o => o.SubOrders)
                .ThenInclude(s => s.Warehouse)
            .Include(o => o.SubOrders)
                .ThenInclude(s => s.SubOrderItems)
                    .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByTenantAsync(Guid tenantId, string? status = null, int limit = 100)
    {
        var query = _context.Orders
            .Include(o => o.SubOrders)
            .Where(o => o.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(o => o.Status == status);
        }

        return await query
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
