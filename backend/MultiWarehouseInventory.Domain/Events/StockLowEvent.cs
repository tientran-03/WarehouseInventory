namespace MultiWarehouseInventory.Domain.Events;

public record StockLowEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "StockLow";
    
    public Guid ProductId { get; }
    public Guid WarehouseId { get; }
    public Guid WarehouseZoneId { get; }
    public int CurrentStock { get; }
    public int ReorderLevel { get; }

    public StockLowEvent(
        Guid productId, 
        Guid warehouseId, 
        Guid warehouseZoneId, 
        int currentStock, 
        int reorderLevel)
    {
        ProductId = productId;
        WarehouseId = warehouseId;
        WarehouseZoneId = warehouseZoneId;
        CurrentStock = currentStock;
        ReorderLevel = reorderLevel;
    }
}