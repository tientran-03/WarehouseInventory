namespace MultiWarehouseInventory.Domain.Entities;

/// <summary>
/// Hóa đơn xuất kho, liên kết với StockDocument loại "Export"
/// </summary>
public class Invoice : TenantEntity
{
    public string InvoiceCode { get; set; } = string.Empty;
    public Guid StockDocumentId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerTaxCode { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Issued"; // Issued, Paid, Cancelled

    public StockDocument StockDocument { get; set; } = null!;
}
