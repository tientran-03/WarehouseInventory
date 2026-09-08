namespace MultiWarehouseInventory.Application.DTOs;

public record CreateTransferRequest(
    Guid TenantId,
    Guid ProductId,
    int Quantity,
    Guid FromWarehouseId,
    Guid ToWarehouseId
);

public record TransferResponse(
    Guid Id,
    Guid TenantId,
    string TransferCode,
    Guid ProductId,
    string ProductName,
    string ProductSku,
    int Quantity,
    Guid FromWarehouseId,
    string FromWarehouseName,
    Guid ToWarehouseId,
    string ToWarehouseName,
    string Status,
    DateTime CreatedAt
);

public record CreateStocktakeRequest(
    Guid TenantId,
    Guid WarehouseId,
    string Auditor
);

public record StocktakeDiscrepancyRequest(
    string ProductId,
    string ProductSku,
    string ProductName,
    int SystemQuantity,
    int ActualQuantity,
    int Discrepancy,
    string Reason
);

public record CompleteStocktakeRequest(
    int DiscrepancyCount,
    IReadOnlyList<StocktakeDiscrepancyRequest> Discrepancies,
    string? Note = null
);

public record StocktakeResponse(
    Guid Id,
    Guid TenantId,
    string StocktakeCode,
    Guid WarehouseId,
    string WarehouseName,
    string Auditor,
    string Status,
    int TotalItems,
    int DiscrepancyCount,
    string? Note,
    DateTime CreatedAt
);

public record CreateZoneRequest(
    Guid TenantId,
    Guid WarehouseId,
    string Code,
    string Name,
    int Capacity
);

public record ZoneResponse(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    string WarehouseName,
    string Code,
    string Name,
    int Capacity,
    int UsedCapacity
);

public record WarehouseFinancialSummary(
    Guid WarehouseId,
    string WarehouseName,
    int TotalUnits,
    decimal TotalValue,
    int LineCount
);

public record FinancialSummaryResponse(
    decimal GrandTotal,
    int WarehouseCount,
    int InventoryLineCount,
    IReadOnlyList<WarehouseFinancialSummary> Warehouses
);

public record BalancingSuggestionResponse(
    Guid ProductId,
    string ProductName,
    string ProductSku,
    string FromWarehouseName,
    string ToWarehouseName,
    int SuggestedQty,
    int Imbalance
);

public record WarehouseCacheDto(
    Guid Id,
    string Name,
    string Code,
    decimal Latitude,
    decimal Longitude,
    bool IsActive
);
