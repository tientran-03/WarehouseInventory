namespace MultiWarehouseInventory.Domain.Entities
{
    public class Product : TenantEntity
    {
        public Guid CategoryId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public decimal Price { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public Tenant Tenant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ICollection<WarehouseInventory> Inventories { get; set; } = new List<WarehouseInventory>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}