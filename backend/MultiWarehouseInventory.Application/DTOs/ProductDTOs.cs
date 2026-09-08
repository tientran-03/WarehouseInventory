namespace MultiWarehouseInventory.Application.DTOs;

public record UpsertProductRequest(
    Guid TenantId,
    Guid CategoryId,
    string Sku,
    string Name,
    decimal Price,
    string? Barcode = null,
    decimal Weight = 0,
    decimal Length = 0,
    decimal Width = 0,
    decimal Height = 0,
    string? ImageUrl = null
);

public record ProductResponse(
    Guid Id,
    Guid TenantId,
    Guid CategoryId,
    string Sku,
    string Name,
    decimal Price,
    string? Barcode,
    string? ImageUrl,
    bool IsActive
);

public record UpsertInventoryRequest(
    Guid TenantId,
    Guid WarehouseId,
    Guid ProductId,
    int OnHandStock,
    int ReorderLevel = 10,
    string? LocationInWarehouse = null,
    Guid? WarehouseZoneId = null
);

public record InventoryResponse(
    Guid Id,
    Guid TenantId,
    Guid WarehouseId,
    string WarehouseName,
    string WarehouseCode,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    int OnHandStock,
    int ReservedStock,
    int AvailableStock,
    int ReorderLevel,
    string? LocationInWarehouse,
    Guid? WarehouseZoneId,
    string? WarehouseZoneCode,
    string? WarehouseZoneName
);

public record CategoryResponse(
    Guid Id,
    Guid TenantId,
    string Name,
    string Slug
);

public record UpsertCategoryRequest(
    Guid TenantId,
    string Name
);
