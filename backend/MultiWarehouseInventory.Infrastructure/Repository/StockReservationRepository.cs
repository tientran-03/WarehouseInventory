using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure.Data;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class StockReservationRepository : Repository<StockReservation>, IStockReservationRepository
{
    public StockReservationRepository(AppDbContext context) : base(context) { }

    public async Task<List<StockReservation>> GetByOrderIdAsync(Guid orderId)
    {
        return await _context.StockReservations
            .Where(r => r.OrderId == orderId)
            .ToListAsync();
    }

    public Task DeleteAsync(StockReservation reservation)
    {
        _context.StockReservations.Remove(reservation);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
