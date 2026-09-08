namespace MultiWarehouseInventory.Domain.Events;

/// <summary>
/// Domain Event khi một order mới được tạo
/// </summary>
public record OrderCreatedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => "OrderCreated";
    
    public Guid OrderId { get; }
    public string OrderCode { get; }
    public Guid TenantId { get; }
    public string Channel { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(
        Guid orderId, 
        string orderCode, 
        Guid tenantId, 
        string channel, 
        decimal totalAmount)
    {
        OrderId = orderId;
        OrderCode = orderCode;
        TenantId = tenantId;
        Channel = channel;
        TotalAmount = totalAmount;
    }
}