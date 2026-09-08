namespace MultiWarehouseInventory.Application.DTOs;

public record CreateInvoiceRequest(
    Guid TenantId,
    Guid StockDocumentId,
    string CustomerName,
    string? CustomerAddress = null,
    string? CustomerPhone = null,
    string? CustomerTaxCode = null,
    decimal TaxRate = 0.1m,
    string? Notes = null
);

public record InvoiceResponse(
    Guid Id,
    Guid TenantId,
    string InvoiceCode,
    Guid StockDocumentId,
    string StockDocumentCode,
    string CustomerName,
    string? CustomerAddress,
    string? CustomerPhone,
    string? CustomerTaxCode,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal TotalAmount,
    string? Notes,
    DateTime InvoiceDate,
    string Status,
    IReadOnlyList<InvoiceLineResponse> Lines
);

public record InvoiceLineResponse(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string WarehouseZoneCode,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);
