
namespace MultiWarehouseInventory.Domain.Entities
{
    public class SubOrder : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid WarehouseId { get; set; }
        public string SubOrderCode { get; set; } = string.Empty;
        public string? ShippingPartner { get; set; }
        public string? TrackingNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public Order Order { get; set; } = null!;
        public Warehouse Warehouse { get; set; } = null!;
        public ICollection<SubOrderItem> SubOrderItems { get; set; } = new List<SubOrderItem>();
    }
}