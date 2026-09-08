namespace MultiWarehouseInventory.Domain.Entities
{
    public class SubOrderItem : BaseEntity
    {
        public Guid SubOrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public SubOrder SubOrder { get; set; } = null!;
        public Product Product { get; set; } = null!;
    }
}