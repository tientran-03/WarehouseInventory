using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IStocktakeRepository
{
    Task<List<Stocktake>> GetByTenantAsync(Guid tenantId);
    Task<Stocktake?> GetByIdAsync(Guid id);
    Task AddAsync(Stocktake stocktake);
    Task UpdateAsync(Stocktake stocktake);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
