using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IStockDocumentService
{
    Task<IReadOnlyList<StockDocumentResponse>> GetByTenantAsync(StockMovementReportFilter filter, CancellationToken cancellationToken = default);
    Task<StockDocumentResponse> CreateAsync(CreateStockDocumentRequest request, Guid createdBy, CancellationToken cancellationToken = default);
    Task<StockMovementReportResponse> GetReportAsync(StockMovementReportFilter filter, CancellationToken cancellationToken = default);
}
