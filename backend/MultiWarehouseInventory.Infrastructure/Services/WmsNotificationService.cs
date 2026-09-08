using Microsoft.Extensions.Logging;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.Infrastructure.Services;

public class WmsNotificationService : IWmsNotificationService
{
    private readonly ILogger<WmsNotificationService> _logger;

    public WmsNotificationService(ILogger<WmsNotificationService> logger)
    {
        _logger = logger;
    }

    public Task SendPickListRequestsAsync(
        OrderAllocationResultDto result,
        CancellationToken cancellationToken = default)
    {
        foreach (var subOrder in result.SubOrders)
        {
            _logger.LogInformation(
                "[WMS] Pick list queued | Order={OrderCode} | SubOrder={SubOrderCode} | Warehouse={WarehouseName} | Items={ItemCount}",
                result.OrderCode,
                subOrder.SubOrderCode,
                subOrder.WarehouseName,
                subOrder.Items.Count);
        }

        return Task.CompletedTask;
    }
}
