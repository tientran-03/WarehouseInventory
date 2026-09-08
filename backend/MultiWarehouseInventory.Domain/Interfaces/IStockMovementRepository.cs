using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IStockMovementRepository
{
    Task AddAsync(StockMovement movement);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
