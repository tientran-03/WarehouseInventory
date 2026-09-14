using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Services;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using Xunit;

namespace MultiWarehouseInventory.Application.Tests;

public class OrderAllocationPlannerTests
{
    [Fact]
    public void Plan_WithEmptyOrderItems_ShouldThrowBadRequestException()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>();
        var warehouses = new List<WarehouseCacheDto>();
        var inventories = new List<WarehouseInventory>();

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(
            () => OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 0, 0));
        
        exception.Message.Should().Contain("ít nhất một sản phẩm");
    }

    [Fact]
    public void Plan_WithNoWarehouses_ShouldThrowBadRequestException()
    {
        // Arrange
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(Guid.NewGuid(), "Product 1", 10)
        };
        var warehouses = new List<WarehouseCacheDto>();
        var inventories = new List<WarehouseInventory>();

        // Act & Assert
        var exception = Assert.Throws<BadRequestException>(
            () => OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 0, 0));
        
        exception.Message.Should().Contain("chưa có kho hoạt động");
    }

    [Fact]
    public void Plan_WithSufficientStock_ShouldAllocateToNearestWarehouse()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(productId, "Product 1", 5)
        };
        
        var warehouses = new List<WarehouseCacheDto>
        {
            new WarehouseCacheDto(warehouseId, "Warehouse 1", "WH001", 21.0285, 105.8542, true)
        };
        
        var inventories = new List<WarehouseInventory>
        {
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                ProductId = productId,
                OnHandStock = 100,
                ReservedStock = 0,
                AvailableStock = 100
            }
        };

        // Act
        var result = OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 21.0278, 105.8342);

        // Assert
        result.Should().HaveCount(1);
        result[0].WarehouseId.Should().Be(warehouseId);
        result[0].ProductId.Should().Be(productId);
        result[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void Plan_WithInsufficientStock_ShouldThrowInsufficientStockException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouseId = Guid.NewGuid();
        
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(productId, "Product 1", 100)
        };
        
        var warehouses = new List<WarehouseCacheDto>
        {
            new WarehouseCacheDto(warehouseId, "Warehouse 1", "WH001", 21.0285, 105.8542, true)
        };
        
        var inventories = new List<WarehouseInventory>
        {
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouseId,
                ProductId = productId,
                OnHandStock = 50,
                ReservedStock = 0,
                AvailableStock = 50
            }
        };

        // Act & Assert
        var exception = Assert.Throws<InsufficientStockException>(
            () => OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 21.0278, 105.8342));
        
        exception.RequestedQuantity.Should().Be(100);
        exception.AvailableQuantity.Should().Be(50);
    }

    [Fact]
    public void Plan_WithMultipleWarehouses_ShouldAllocateToNearestFirst()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouse1Id = Guid.NewGuid();
        var warehouse2Id = Guid.NewGuid();
        
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(productId, "Product 1", 10)
        };
        
        var warehouses = new List<WarehouseCacheDto>
        {
            new WarehouseCacheDto(warehouse1Id, "Warehouse 1", "WH001", 21.0285, 105.8542, true), // ~2km away
            new WarehouseCacheDto(warehouse2Id, "Warehouse 2", "WH002", 21.0500, 105.9000, true) // ~10km away
        };
        
        var inventories = new List<WarehouseInventory>
        {
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse1Id,
                ProductId = productId,
                OnHandStock = 100,
                ReservedStock = 0,
                AvailableStock = 100
            },
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse2Id,
                ProductId = productId,
                OnHandStock = 100,
                ReservedStock = 0,
                AvailableStock = 100
            }
        };

        // Act
        var result = OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 21.0278, 105.8342);

        // Assert
        result.Should().HaveCount(1);
        result[0].WarehouseId.Should().Be(warehouse1Id); // Should allocate to nearest warehouse
    }

    [Fact]
    public void Plan_WithSplitOrder_ShouldAllocateAcrossMultipleWarehouses()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var warehouse1Id = Guid.NewGuid();
        var warehouse2Id = Guid.NewGuid();
        
        var orderItems = new List<OrderItemDto>
        {
            new OrderItemDto(productId, "Product 1", 15)
        };
        
        var warehouses = new List<WarehouseCacheDto>
        {
            new WarehouseCacheDto(warehouse1Id, "Warehouse 1", "WH001", 21.0285, 105.8542, true),
            new WarehouseCacheDto(warehouse2Id, "Warehouse 2", "WH002", 21.0500, 105.9000, true)
        };
        
        var inventories = new List<WarehouseInventory>
        {
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse1Id,
                ProductId = productId,
                OnHandStock = 10,
                ReservedStock = 0,
                AvailableStock = 10
            },
            new WarehouseInventory
            {
                Id = Guid.NewGuid(),
                WarehouseId = warehouse2Id,
                ProductId = productId,
                OnHandStock = 10,
                ReservedStock = 0,
                AvailableStock = 10
            }
        };

        // Act
        var result = OrderAllocationPlanner.Plan(orderItems, warehouses, inventories, 21.0278, 105.8342);

        // Assert
        result.Should().HaveCount(2);
        result.Sum(r => r.Quantity).Should().Be(15);
        result[0].WarehouseId.Should().Be(warehouse1Id); // Nearest gets allocated first
        result[0].Quantity.Should().Be(10);
        result[1].WarehouseId.Should().Be(warehouse2Id);
        result[1].Quantity.Should().Be(5);
    }
}
