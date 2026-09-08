namespace MultiWarehouseInventory.Application.DTOs;

public record OrderAllocationResultDto(
    Guid OrderId,
    string OrderCode,
    string Status,
    decimal CustomerLatitude,
    decimal CustomerLongitude,
    int SubOrderCount,
    bool IsSplitOrder,
    IReadOnlyList<SubOrderAllocationDto> SubOrders
);

public record SubOrderAllocationDto(
    Guid SubOrderId,
    string SubOrderCode,
    Guid WarehouseId,
    string WarehouseName,
    double DistanceKm,
    IReadOnlyList<SubOrderItemAllocationDto> Items
);

public record SubOrderItemAllocationDto(
    Guid ProductId,
    int Quantity
);
