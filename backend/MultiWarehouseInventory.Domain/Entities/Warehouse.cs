
namespace MultiWarehouseInventory.Domain.Entities
{
    public class Warehouse : TenantEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsActive { get; set; } = true;
        public Tenant Tenant { get; set; } = null!;
        public ICollection<WarehouseInventory> Inventories { get; set; } = new List<WarehouseInventory>();
        public ICollection<SubOrder> SubOrders { get; set; } = new List<SubOrder>();
    }
}