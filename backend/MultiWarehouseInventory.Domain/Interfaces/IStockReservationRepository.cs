using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IStockReservationRepository
{
    Task AddAsync(StockReservation reservation);
    Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId);
    Task DeleteAsync(StockReservation reservation);
    Task SaveChangesAsync();
}
