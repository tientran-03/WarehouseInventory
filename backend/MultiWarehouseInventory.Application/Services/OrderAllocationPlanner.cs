using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.Application.Services;

internal sealed class AllocationLine
{
    public Guid WarehouseId { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public double DistanceKm { get; init; }
}

internal static class OrderAllocationPlanner
{
    public static IReadOnlyList<AllocationLine> Plan(
        IReadOnlyList<OrderItemDto> orderItems,
        IReadOnlyList<WarehouseCacheDto> warehouses,
        IReadOnlyList<WarehouseInventory> inventories,
        decimal customerLatitude,
        decimal customerLongitude)
    {
        if (orderItems.Count == 0)
        {
            throw new BadRequestException("Đơn hàng phải có ít nhất một sản phẩm.");
        }

        if (warehouses.Count == 0)
        {
            throw new BadRequestException("Tenant chưa có kho hoạt động để phân bổ đơn hàng.");
        }

        var remaining = orderItems.ToDictionary(x => x.ProductId, x => x.Quantity);
        var available = inventories
            .GroupBy(x => (x.WarehouseId, x.ProductId))
            .ToDictionary(
                group => group.Key,
                group => group.Sum(x => x.AvailableStock));

        var warehousesByDistance = warehouses
            .Select(w => new
            {
                Warehouse = w,
                DistanceKm = DistanceCalculator.CalculateDistance(
                    customerLatitude,
                    customerLongitude,
                    w.Latitude,
                    w.Longitude),
            })
            .OrderBy(x => x.DistanceKm)
            .ToList();

        var allocations = new List<AllocationLine>();

        foreach (var entry in warehousesByDistance)
        {
            foreach (var item in orderItems)
            {
                if (remaining[item.ProductId] <= 0)
                {
                    continue;
                }

                if (!available.TryGetValue((entry.Warehouse.Id, item.ProductId), out var stock) || stock <= 0)
                {
                    continue;
                }

                var take = Math.Min(remaining[item.ProductId], stock);
                allocations.Add(new AllocationLine
                {
                    WarehouseId = entry.Warehouse.Id,
                    ProductId = item.ProductId,
                    Quantity = take,
                    DistanceKm = entry.DistanceKm,
                });

                remaining[item.ProductId] -= take;
                available[(entry.Warehouse.Id, item.ProductId)] = stock - take;
            }

            if (remaining.Values.All(q => q == 0))
            {
                break;
            }
        }

        foreach (var item in orderItems)
        {
            if (remaining[item.ProductId] > 0)
            {
                var totalAvailable = inventories
                    .Where(i => i.ProductId == item.ProductId)
                    .Sum(i => i.AvailableStock);

                throw new InsufficientStockException(
                    Guid.Empty,
                    item.ProductId,
                    item.Quantity,
                    totalAvailable);
            }
        }

        return allocations;
    }
}
