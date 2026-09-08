using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IOrderNotificationService
{
    Task NotifyWarehousesAsync(OrderAllocationResultDto result, CancellationToken cancellationToken = default);
}
