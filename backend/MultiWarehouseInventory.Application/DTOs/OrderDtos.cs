namespace MultiWarehouseInventory.Application.DTOs
{
    public record CreateOrderRequest(
        Guid TenantId,
        string OrderCode,
        string Channel,
        string CustomerName,
        string CustomerPhone,
        string CustomerAddress,
        decimal CustomerLatitude,
        decimal CustomerLongitude,
        decimal ShippingFee,
        List<OrderItemDto> OrderItems
    );

    public record OrderItemDto(Guid ProductId, int Quantity, decimal UnitPrice);

    public record UpdateOrderStatusRequest(string Status);
}