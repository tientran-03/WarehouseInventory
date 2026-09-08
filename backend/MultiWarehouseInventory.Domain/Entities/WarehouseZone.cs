namespace MultiWarehouseInventory.Domain.Entities;

public class WarehouseZone : TenantEntity
{
    public Guid WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int UsedCapacity { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<WarehouseInventory> Inventories { get; set; } = new List<WarehouseInventory>();
}
