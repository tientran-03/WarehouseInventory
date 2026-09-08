namespace MultiWarehouseInventory.Application.DTOs;

public static class StockDocumentTypes
{
    public const string Inbound = "Inbound";
    public const string Outbound = "Outbound";
    public const string OpeningBalance = "OpeningBalance";
    public const string AdjustmentIn = "AdjustmentIn";
    public const string AdjustmentOut = "AdjustmentOut";
}

public record CreateStockDocumentLineRequest(
    Guid ProductId,
    Guid WarehouseZoneId,
    int Quantity,
    decimal? UnitPrice = null
);

public record CreateStockDocumentRequest(
    Guid TenantId,
    Guid WarehouseId,
    string Type,
    DateTime? DocumentDate,
    string? PartnerName,
    string? ReferenceCode,
    string? Note,
    IReadOnlyList<CreateStockDocumentLineRequest> Lines
);

public record StockDocumentLineResponse(
    Guid Id,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    Guid WarehouseZoneId,
    string WarehouseZoneCode,
    string WarehouseZoneName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

public record StockDocumentResponse(
    Guid Id,
    Guid TenantId,
    string DocumentCode,
    string Type,
    string Status,
    Guid WarehouseId,
    string WarehouseName,
    DateTime DocumentDate,
    string? PartnerName,
    string? ReferenceCode,
    string? Note,
    Guid CreatedBy,
    DateTime CreatedAt,
    IReadOnlyList<StockDocumentLineResponse> Lines,
    int TotalQuantity,
    decimal TotalValue
);

public record StockMovementReportFilter(
    Guid TenantId,
    Guid? WarehouseId = null,
    string? Type = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null
);

public record StockMovementReportRow(
    DateTime DocumentDate,
    string DocumentCode,
    string Type,
    string WarehouseName,
    string ProductSku,
    string ProductName,
    string WarehouseZoneCode,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    string? PartnerName,
    string? ReferenceCode,
    string? Note
);

public record StockMovementReportResponse(
    DateTime? FromDate,
    DateTime? ToDate,
    int DocumentCount,
    int InboundQuantity,
    int OutboundQuantity,
    decimal InboundValue,
    decimal OutboundValue,
    int OpeningBalanceQuantity,
    int AdjustmentInQuantity,
    int AdjustmentOutQuantity,
    decimal OpeningBalanceValue,
    decimal AdjustmentInValue,
    decimal AdjustmentOutValue,
    IReadOnlyList<StockMovementReportRow> Rows
);
