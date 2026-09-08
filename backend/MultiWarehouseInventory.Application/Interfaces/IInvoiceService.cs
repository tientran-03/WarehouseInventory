using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request);
    Task<InvoiceResponse?> GetByIdAsync(Guid id);
    Task<IEnumerable<InvoiceResponse>> GetByTenantAsync(Guid tenantId);
    Task<InvoiceResponse?> GetByStockDocumentIdAsync(Guid stockDocumentId);
}
