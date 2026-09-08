namespace MultiWarehouseInventory.Domain.Entities;

public class WarehouseTransfer : TenantEntity
{
    public string TransferCode { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Guid FromWarehouseId { get; set; }
    public Guid ToWarehouseId { get; set; }
    public string Status { get; set; } = "Pending";

    public Product Product { get; set; } = null!;
    public Warehouse FromWarehouse { get; set; } = null!;
    public Warehouse ToWarehouse { get; set; } = null!;
}
