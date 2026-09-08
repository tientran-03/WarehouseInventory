
namespace MultiWarehouseInventory.Domain.Entities
{
    public class StockMovement : TenantEntity
    {
        public Guid WarehouseId { get; set; }
        public Guid ProductId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int QuantityChanged { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        public string? ReferenceCode { get; set; }
        public string? Note { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? StockDocumentId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public StockDocument? StockDocument { get; set; }
    }
}
