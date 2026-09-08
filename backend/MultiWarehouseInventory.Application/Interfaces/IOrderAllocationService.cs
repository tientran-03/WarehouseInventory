using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IOrderAllocationService
{
    Task<OrderAllocationResultDto> ProcessIncomingOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default);
}
