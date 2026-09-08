using Microsoft.AspNetCore.SignalR;

namespace MultiWarehouseInventory.API.Hubs;

public class WarehouseOrderHub : Hub
{
    public static string GetWarehouseGroupName(Guid warehouseId) => $"warehouse-{warehouseId}";

    public async Task JoinWarehouse(Guid warehouseId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GetWarehouseGroupName(warehouseId));
    }

    public async Task LeaveWarehouse(Guid warehouseId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetWarehouseGroupName(warehouseId));
    }
}
