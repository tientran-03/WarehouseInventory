using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Services;
using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Application.Tests;

public class OrderAllocationPlannerTests
{
    [Fact]
    public void Plan_SplitsOrder_WhenNearestWarehouseHasPartialStock()
    {
        var tenantId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var nearWarehouseId = Guid.NewGuid();
        var farWarehouseId = Guid.NewGuid();

        var warehouses = new List<WarehouseCacheDto>
        {
            new(nearWarehouseId, tenantId, "WH-HN", "Kho Hà Nội", 21.0285m, 105.8542m),
            new(farWarehouseId, tenantId, "WH-HCM", "Kho TP.HCM", 10.8231m, 106.6297m),
        };

        var inventories = new List<WarehouseInventory>
        {
            new()
            {
                WarehouseId = nearWarehouseId,
                ProductId = productId,
                TenantId = tenantId,
                OnHandStock = 3,
                ReservedStock = 0,
            },
            new()
            {
                WarehouseId = farWarehouseId,
                ProductId = productId,
                TenantId = tenantId,
                OnHandStock = 10,
                ReservedStock = 0,
            },
        };

        var items = new List<OrderItemDto>
        {
            new(productId, 8, 100_000m),
        };

        var lines = OrderAllocationPlanner.Plan(
            items,
            warehouses,
            inventories,
            21.03m,
            105.85m);

        Assert.Equal(2, lines.Count);
        Assert.Equal(3, lines.Single(x => x.WarehouseId == nearWarehouseId).Quantity);
        Assert.Equal(5, lines.Single(x => x.WarehouseId == farWarehouseId).Quantity);
    }
}
