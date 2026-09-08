namespace MultiWarehouseInventory.Domain.Events;

public record StockReservedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "StockReserved";
    
    public Guid OrderId { get; }
    public Guid ProductId { get; }
    public Guid WarehouseId { get; }
    public int Quantity { get; }
    public Guid WarehouseZoneId { get; }

    public StockReservedEvent(
        Guid orderId, 
        Guid productId, 
        Guid warehouseId, 
        int quantity, 
        Guid warehouseZoneId)
    {
        OrderId = orderId;
        ProductId = productId;
        WarehouseId = warehouseId;
        Quantity = quantity;
        WarehouseZoneId = warehouseZoneId;
    }
}