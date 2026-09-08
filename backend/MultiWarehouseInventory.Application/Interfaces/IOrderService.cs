using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IOrderService
{
    Task<IEnumerable<OrderListItemDto>> GetByTenantAsync(Guid tenantId, string? status = null);

    Task<OrderDetailDto> GetByIdAsync(Guid id);

    Task<OrderDetailDto> UpdateStatusAsync(Guid id, string status);

    Task<OrderDetailDto> CreateStockDocumentsFromOrderAsync(Guid orderId, Guid createdBy);
}
