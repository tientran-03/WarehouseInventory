using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<Order?> GetByOrderCodeAsync(Guid tenantId, string orderCode);
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<List<Order>> GetByTenantAsync(Guid tenantId, string? status = null, int limit = 100);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
