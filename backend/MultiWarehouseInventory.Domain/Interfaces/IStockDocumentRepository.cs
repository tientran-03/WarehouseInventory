using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IStockDocumentRepository
{
    Task AddAsync(StockDocument document, CancellationToken cancellationToken = default);

    Task<StockDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<StockDocument>> GetByTenantAsync(
        Guid tenantId,
        Guid? warehouseId = null,
        string? type = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}
