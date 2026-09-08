using System.Text.Json;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class StocktakeService : IStocktakeService
{
    private readonly IStocktakeRepository _stocktakeRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseInventoryRepository _inventoryRepository;

    public StocktakeService(
        IStocktakeRepository stocktakeRepository,
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IWarehouseInventoryRepository inventoryRepository)
    {
        _stocktakeRepository = stocktakeRepository;
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IEnumerable<StocktakeResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantAsync(tenantId);
        var items = await _stocktakeRepository.GetByTenantAsync(tenantId);
        return items.Select(Map);
    }

    public async Task<StocktakeResponse> CreateAsync(CreateStocktakeRequest request)
    {
        await EnsureTenantAsync(request.TenantId);

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse is null || !warehouse.IsActive || warehouse.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        if (string.IsNullOrWhiteSpace(request.Auditor))
        {
            throw new BadRequestException("Người kiểm kê không được để trống.");
        }

        var inventoryLines = await _inventoryRepository.GetByWarehouseAsync(request.WarehouseId);

        var stocktake = new Stocktake
        {
            TenantId = request.TenantId,
            StocktakeCode = $"STK-{DateTime.UtcNow:yyyyMMddHHmmss}",
            WarehouseId = request.WarehouseId,
            Auditor = request.Auditor.Trim(),
            Status = "InProgress",
            TotalItems = inventoryLines.Count,
            DiscrepancyCount = 0,
            CreatedAt = DateTime.UtcNow,
        };

        await _stocktakeRepository.AddAsync(stocktake);
        await _stocktakeRepository.SaveChangesAsync();

        var saved = await _stocktakeRepository.GetByIdAsync(stocktake.Id)
            ?? throw new InvalidOperationException("Không thể tải phiên kiểm kê vừa tạo.");
        return Map(saved);
    }

    public async Task<StocktakeResponse> CompleteAsync(Guid id, CompleteStocktakeRequest request)
    {
        var stocktake = await _stocktakeRepository.GetByIdAsync(id);
        if (stocktake is null)
        {
            throw new NotFoundException(nameof(Stocktake), id);
        }

        if (stocktake.Status == "Completed")
        {
            throw new BadRequestException("Phiên kiểm kê đã hoàn thành.");
        }

        stocktake.Status = "Completed";
        stocktake.DiscrepancyCount = Math.Max(0, request.DiscrepancyCount);
        stocktake.Note = request.Note;
        stocktake.DiscrepanciesJson = request.Discrepancies.Count > 0 
            ? JsonSerializer.Serialize(request.Discrepancies) 
            : null;
        stocktake.UpdatedAt = DateTime.UtcNow;

        await _stocktakeRepository.UpdateAsync(stocktake);
        await _stocktakeRepository.SaveChangesAsync();

        return Map(await _stocktakeRepository.GetByIdAsync(id) ?? stocktake);
    }

    private async Task EnsureTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static StocktakeResponse Map(Stocktake s) => new(
        s.Id,
        s.TenantId,
        s.StocktakeCode,
        s.WarehouseId,
        s.Warehouse?.Name ?? string.Empty,
        s.Auditor,
        s.Status,
        s.TotalItems,
        s.DiscrepancyCount,
        s.Note,
        s.CreatedAt);
}
