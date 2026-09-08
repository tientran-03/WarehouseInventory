using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseZoneRepository _warehouseZoneRepository;

    public InventoryService(
        IWarehouseInventoryRepository inventoryRepository,
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseZoneRepository warehouseZoneRepository)
    {
        _inventoryRepository = inventoryRepository;
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _warehouseZoneRepository = warehouseZoneRepository;
    }

    public async Task<IEnumerable<InventoryResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantExistsAsync(tenantId);
        var items = await _inventoryRepository.GetByTenantAsync(tenantId);
        return items.Select(MapToResponse);
    }

    public async Task<IEnumerable<InventoryResponse>> GetByWarehouseAsync(Guid warehouseId)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
        if (warehouse is null || !warehouse.IsActive)
        {
            throw new NotFoundException(nameof(Warehouse), warehouseId);
        }

        var items = await _inventoryRepository.GetByWarehouseAsync(warehouseId);
        return items.Select(MapToResponse);
    }

    public async Task<InventoryResponse> UpsertAsync(UpsertInventoryRequest request)
    {
        await EnsureTenantExistsAsync(request.TenantId);

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse is null || !warehouse.IsActive || warehouse.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null || !product.IsActive || product.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        if (request.OnHandStock < 0)
        {
            throw new BadRequestException("OnHandStock không được âm.");
        }

        if (!request.WarehouseZoneId.HasValue)
        {
            throw new BadRequestException("Vui lòng chọn khu vực/vị trí lưu trữ.");
        }

        var zone = await _warehouseZoneRepository.GetByIdAsync(request.WarehouseZoneId.Value);
        if (zone is null || zone.TenantId != request.TenantId || zone.WarehouseId != request.WarehouseId)
        {
            throw new NotFoundException(nameof(WarehouseZone), request.WarehouseZoneId.Value);
        }

        var existing = await _inventoryRepository.GetByWarehouseProductAndZoneAsync(
            request.WarehouseId,
            request.ProductId,
            request.TenantId,
            zone.Id);

        var currentZoneStock = await _inventoryRepository.GetOnHandStockByZoneAsync(zone.Id);
        var projectedZoneStock = currentZoneStock - (existing?.OnHandStock ?? 0) + request.OnHandStock;
        if (projectedZoneStock > zone.Capacity)
        {
            throw new BadRequestException(
                $"Khu vực {zone.Code} chỉ còn {Math.Max(0, zone.Capacity - currentZoneStock + (existing?.OnHandStock ?? 0))} sức chứa.");
        }

        if (existing is null)
        {
            existing = new WarehouseInventory
            {
                TenantId = request.TenantId,
                WarehouseId = request.WarehouseId,
                ProductId = request.ProductId,
                OnHandStock = request.OnHandStock,
                ReservedStock = 0,
                ReorderLevel = request.ReorderLevel,
                WarehouseZoneId = zone.Id,
                LocationInWarehouse = zone.Code,
                CreatedAt = DateTime.UtcNow,
            };

            await _inventoryRepository.AddAsync(existing);
        }
        else
        {
            if (request.OnHandStock < existing.ReservedStock)
            {
                throw new BadRequestException(
                    $"OnHandStock ({request.OnHandStock}) không thể nhỏ hơn ReservedStock ({existing.ReservedStock}).");
            }

            existing.OnHandStock = request.OnHandStock;
            existing.ReorderLevel = request.ReorderLevel;
            existing.LocationInWarehouse = zone.Code;
            existing.UpdatedAt = DateTime.UtcNow;

            await _inventoryRepository.UpdateAsync(existing);
        }

        zone.UsedCapacity = projectedZoneStock;
        zone.UpdatedAt = DateTime.UtcNow;
        await _warehouseZoneRepository.UpdateAsync(zone);
        await _inventoryRepository.SaveChangesAsync();

        var saved = await _inventoryRepository.GetByIdAsync(existing.Id)
            ?? throw new InvalidOperationException("Không thể tải lại bản ghi tồn kho vừa lưu.");

        return MapToResponse(saved);
    }

    public async Task<InventoryResponse> UpdateAsync(Guid id, UpsertInventoryRequest request)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory is null)
        {
            throw new NotFoundException(nameof(WarehouseInventory), id);
        }

        if (inventory.TenantId != request.TenantId)
        {
            throw new BadRequestException("Không thể chuyển bản ghi tồn kho sang tenant khác.");
        }

        await EnsureTenantExistsAsync(request.TenantId);

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse is null || !warehouse.IsActive || warehouse.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product is null || !product.IsActive || product.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }

        if (request.OnHandStock < inventory.ReservedStock)
        {
            throw new BadRequestException(
                $"OnHandStock ({request.OnHandStock}) không thể nhỏ hơn ReservedStock ({inventory.ReservedStock}).");
        }

        if (!request.WarehouseZoneId.HasValue)
        {
            throw new BadRequestException("Vui lòng chọn khu vực/vị trí lưu trữ.");
        }

        var targetZone = await _warehouseZoneRepository.GetByIdAsync(request.WarehouseZoneId.Value);
        if (targetZone is null || targetZone.TenantId != request.TenantId || targetZone.WarehouseId != request.WarehouseId)
        {
            throw new NotFoundException(nameof(WarehouseZone), request.WarehouseZoneId.Value);
        }

        var duplicate = await _inventoryRepository.GetByWarehouseProductAndZoneAsync(
            request.WarehouseId,
            request.ProductId,
            request.TenantId,
            targetZone.Id);
        if (duplicate is not null && duplicate.Id != inventory.Id)
        {
            throw new BadRequestException("Sản phẩm này đã có tồn kho tại khu vực đã chọn.");
        }

        var oldZone = inventory.WarehouseZone;
        var sameZone = inventory.WarehouseZoneId == targetZone.Id;
        var targetZoneStock = await _inventoryRepository.GetOnHandStockByZoneAsync(targetZone.Id);
        var projectedTargetZoneStock = targetZoneStock - (sameZone ? inventory.OnHandStock : 0) + request.OnHandStock;
        if (projectedTargetZoneStock > targetZone.Capacity)
        {
            throw new BadRequestException(
                $"Khu vực {targetZone.Code} chỉ còn {Math.Max(0, targetZone.Capacity - targetZoneStock + (sameZone ? inventory.OnHandStock : 0))} sức chứa.");
        }

        if (!sameZone && oldZone is not null)
        {
            var oldZoneStock = await _inventoryRepository.GetOnHandStockByZoneAsync(oldZone.Id);
            oldZone.UsedCapacity = Math.Max(0, oldZoneStock - inventory.OnHandStock);
            oldZone.UpdatedAt = DateTime.UtcNow;
            await _warehouseZoneRepository.UpdateAsync(oldZone);
        }

        inventory.WarehouseId = request.WarehouseId;
        inventory.ProductId = request.ProductId;
        inventory.ReorderLevel = request.ReorderLevel;
        inventory.WarehouseZoneId = targetZone.Id;
        inventory.LocationInWarehouse = targetZone.Code;
        inventory.UpdatedAt = DateTime.UtcNow;

        // Use domain method to update stock
        try
        {
            var stockDifference = request.OnHandStock - inventory.OnHandStock;
            if (stockDifference > 0)
            {
                inventory.AddStock(stockDifference);
            }
            else if (stockDifference < 0)
            {
                inventory.RemoveStock(Math.Abs(stockDifference));
            }
        }
        catch (DomainException ex)
        {
            throw new BadRequestException(ex.Message);
        }

        targetZone.UsedCapacity = projectedTargetZoneStock;
        targetZone.UpdatedAt = DateTime.UtcNow;
        await _warehouseZoneRepository.UpdateAsync(targetZone);
        await _inventoryRepository.UpdateAsync(inventory);
        await _inventoryRepository.SaveChangesAsync();

        var saved = await _inventoryRepository.GetByIdAsync(id)
            ?? throw new InvalidOperationException("Không thể tải lại bản ghi tồn kho vừa cập nhật.");

        return MapToResponse(saved);
    }

    public async Task DeleteAsync(Guid id)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(id);
        if (inventory is null)
        {
            throw new NotFoundException(nameof(WarehouseInventory), id);
        }

        if (inventory.ReservedStock > 0)
        {
            throw new BadRequestException("Không thể xóa tồn kho đang có hàng được giữ cho đơn hàng.");
        }

        WarehouseZone? zone = null;
        var updatedZoneUsage = 0;
        if (inventory.WarehouseZoneId.HasValue)
        {
            zone = inventory.WarehouseZone ?? await _warehouseZoneRepository.GetByIdAsync(inventory.WarehouseZoneId.Value);
            if (zone is not null)
            {
                var currentZoneStock = await _inventoryRepository.GetOnHandStockByZoneAsync(zone.Id);
                updatedZoneUsage = Math.Max(0, currentZoneStock - inventory.OnHandStock);
            }
        }

        await _inventoryRepository.DeleteAsync(inventory);
        if (zone is not null)
        {
            zone.UsedCapacity = updatedZoneUsage;
            zone.UpdatedAt = DateTime.UtcNow;
            await _warehouseZoneRepository.UpdateAsync(zone);
        }
        await _inventoryRepository.SaveChangesAsync();
    }

    private async Task EnsureTenantExistsAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static InventoryResponse MapToResponse(WarehouseInventory item)
    {
        return new InventoryResponse(
            item.Id,
            item.TenantId,
            item.WarehouseId,
            item.Warehouse?.Name ?? string.Empty,
            item.Warehouse?.Code ?? string.Empty,
            item.ProductId,
            item.Product?.Sku ?? string.Empty,
            item.Product?.Name ?? string.Empty,
            item.OnHandStock,
            item.ReservedStock,
            item.AvailableStock,
            item.ReorderLevel,
            item.LocationInWarehouse,
            item.WarehouseZoneId,
            item.WarehouseZone?.Code,
            item.WarehouseZone?.Name);
    }
}
