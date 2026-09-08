namespace MultiWarehouseInventory.Domain.Entities;

public class Stocktake : TenantEntity
{
    public string StocktakeCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Auditor { get; set; } = string.Empty;
    public string Status { get; set; } = "InProgress";
    public int TotalItems { get; set; }
    public int DiscrepancyCount { get; set; }
    public string? Note { get; set; }
    public string? DiscrepanciesJson { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
}
