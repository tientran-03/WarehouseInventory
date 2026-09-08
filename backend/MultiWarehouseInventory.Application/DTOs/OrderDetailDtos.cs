namespace MultiWarehouseInventory.Application.DTOs;

public record OrderListItemDto(
    Guid Id,
    string OrderCode,
    string Channel,
    string CustomerName,
    string Status,
    decimal TotalAmount,
    int SubOrderCount,
    bool IsSplitOrder,
    DateTime CreatedAt
);

public record OrderItemDetailDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record SubOrderDetailDto(
    Guid Id,
    string SubOrderCode,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    string Status,
    IReadOnlyList<SubOrderItemDetailDto> Items
);

public record SubOrderItemDetailDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int Quantity
);

public record OrderDetailDto(
    Guid Id,
    Guid TenantId,
    string OrderCode,
    string Channel,
    string CustomerName,
    string CustomerPhone,
    string CustomerAddress,
    decimal CustomerLatitude,
    decimal CustomerLongitude,
    decimal TotalAmount,
    decimal ShippingFee,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<OrderItemDetailDto> OrderItems,
    IReadOnlyList<SubOrderDetailDto> SubOrders
);
