using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Events;

namespace MultiWarehouseInventory.Domain.Entities
{
    public class Order : TenantEntity
    {
        private const decimal MaxOrderAmount = 100_000_000; // 100 triệu VND
        
        public string OrderCode { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public decimal CustomerLatitude { get; set; }
        public decimal CustomerLongitude { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public string Status { get; set; } = string.Empty;
        public Tenant Tenant { get; set; } = null!;
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
        public ICollection<StockReservation> StockReservations { get; set; } = new List<StockReservation>();

        // Domain Methods (optional - can be used for validation)
        public void ValidateTotalAmount()
        {
            if (TotalAmount < 0)
                throw new DomainException("Tổng đơn hàng không được âm.");
            if (TotalAmount > MaxOrderAmount)
                throw new DomainException($"Tổng đơn hàng không được vượt quá {MaxOrderAmount}.");
        }

        public bool CanBeAllocated => Status == "Pending" && OrderItems.Count > 0;
        public bool IsAllocated => Status == "Allocated";
        public bool IsCompleted => Status == "Completed";
        public bool IsCancelled => Status == "Cancelled";

        // Domain event for order creation
        public void EmitOrderCreatedEvent()
        {
            AddDomainEvent(new OrderCreatedEvent(Id, OrderCode, TenantId, Channel, TotalAmount));
        }
    }
}