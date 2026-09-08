using Microsoft.AspNetCore.SignalR;
using MultiWarehouseInventory.API.Hubs;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Services;

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IHubContext<WarehouseOrderHub> _hubContext;
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(
        IHubContext<WarehouseOrderHub> hubContext,
        ILogger<OrderNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyWarehousesAsync(
        OrderAllocationResultDto result,
        CancellationToken cancellationToken = default)
    {
        foreach (var subOrder in result.SubOrders)
        {
            var payload = new
            {
                result.OrderId,
                result.OrderCode,
                subOrder.SubOrderId,
                subOrder.SubOrderCode,
                subOrder.WarehouseId,
                subOrder.WarehouseName,
                subOrder.DistanceKm,
                subOrder.Items,
                ReceivedAt = DateTime.UtcNow,
            };

            await _hubContext.Clients
                .Group(WarehouseOrderHub.GetWarehouseGroupName(subOrder.WarehouseId))
                .SendAsync("NewSubOrder", payload, cancellationToken);

            _logger.LogInformation(
                "SignalR notification sent to warehouse {WarehouseId} for sub-order {SubOrderCode}",
                subOrder.WarehouseId,
                subOrder.SubOrderCode);
        }
    }
}
