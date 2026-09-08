using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IWmsNotificationService
{
    Task SendPickListRequestsAsync(OrderAllocationResultDto result, CancellationToken cancellationToken = default);
}
