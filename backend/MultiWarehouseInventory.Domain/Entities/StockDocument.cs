namespace MultiWarehouseInventory.Domain.Entities;

/// <summary>
/// Phiếu nghiệp vụ nhập hoặc xuất kho. Phiếu đã hoàn thành không được sửa/xóa
/// để số liệu tồn kho và lịch sử kiểm toán luôn có thể đối chiếu được.
/// </summary>
public class StockDocument : TenantEntity
{
    public string DocumentCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "Completed";
    public DateTime DocumentDate { get; set; } = DateTime.UtcNow;
    public string? PartnerName { get; set; }
    public string? ReferenceCode { get; set; }
    public string? Note { get; set; }
    public Guid CreatedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;
    public ICollection<StockDocumentLine> Lines { get; set; } = new List<StockDocumentLine>();
    public ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();
}

/// <summary>
/// Một dòng hàng của phiếu nhập/xuất. Khu vực kho được lưu ở cấp dòng để
/// việc cập nhật tồn và sức chứa luôn xác định rõ vị trí hàng hóa.
/// </summary>
public class StockDocumentLine : BaseEntity
{
    public Guid StockDocumentId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseZoneId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    public StockDocument StockDocument { get; set; } = null!;
    public Product Product { get; set; } = null!;
    public WarehouseZone WarehouseZone { get; set; } = null!;
}
