namespace MultiWarehouseInventory.Domain.Entities
{
    public class StockReservation : TenantEntity
    {
        public Guid WarehouseId { get; set; }
        public Guid ProductId { get; set; }
        public Guid OrderId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}