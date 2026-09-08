using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly IProductRepository _productRepository;

    public AnalyticsService(
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IWarehouseInventoryRepository inventoryRepository,
        IProductRepository productRepository)
    {
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
        _productRepository = productRepository;
    }

    public async Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid tenantId)
    {
        await EnsureTenantAsync(tenantId);

        var warehouses = await _warehouseRepository.GetActiveWarehousesByTenantAsync(tenantId);
        var inventory = await _inventoryRepository.GetByTenantAsync(tenantId);
        var products = (await _productRepository.GetByTenantAsync(tenantId)).ToDictionary(p => p.Id, p => p.Price);

        var summaries = warehouses.Select(wh =>
        {
            var lines = inventory.Where(i => i.WarehouseId == wh.Id).ToList();
            var totalUnits = lines.Sum(i => i.OnHandStock);
            var totalValue = lines.Sum(i => i.OnHandStock * products.GetValueOrDefault(i.ProductId, 0));
            return new WarehouseFinancialSummary(wh.Id, wh.Name, totalUnits, totalValue, lines.Count);
        }).ToList();

        return new FinancialSummaryResponse(
            summaries.Sum(s => s.TotalValue),
            warehouses.Count,
            inventory.Count,
            summaries);
    }

    public async Task<IEnumerable<BalancingSuggestionResponse>> GetBalancingSuggestionsAsync(Guid tenantId)
    {
        await EnsureTenantAsync(tenantId);

        var inventory = await _inventoryRepository.GetByTenantAsync(tenantId);
        var byProduct = inventory.GroupBy(i => i.ProductId);

        var suggestions = new List<BalancingSuggestionResponse>();

        foreach (var group in byProduct)
        {
            var stocks = group.ToList();
            if (stocks.Count < 2) continue;

            var max = stocks.MaxBy(s => s.AvailableStock)!;
            var min = stocks.MinBy(s => s.AvailableStock)!;
            var imbalance = max.AvailableStock - min.AvailableStock;
            var suggested = imbalance / 2;

            if (suggested <= 0) continue;

            suggestions.Add(new BalancingSuggestionResponse(
                group.Key,
                max.Product?.Name ?? string.Empty,
                max.Product?.Sku ?? string.Empty,
                max.Warehouse?.Name ?? string.Empty,
                min.Warehouse?.Name ?? string.Empty,
                suggested,
                imbalance));
        }

        return suggestions.OrderByDescending(s => s.Imbalance);
    }

    private async Task EnsureTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Domain.Entities.Tenant), tenantId);
        }
    }
}
