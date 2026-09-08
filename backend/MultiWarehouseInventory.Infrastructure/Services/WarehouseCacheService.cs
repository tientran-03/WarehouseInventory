 using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Interfaces;
using System.Text.Json;

namespace MultiWarehouseInventory.Infrastructure.Services;

public class WarehouseCacheService : IWarehouseCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly ILogger<WarehouseCacheService> _logger;
    private readonly DistributedCacheEntryOptions _cacheOptions;

    public WarehouseCacheService(
        IDistributedCache cache,
        IWarehouseRepository warehouseRepository,
        ILogger<WarehouseCacheService> logger)
    {
        _cache = cache;
        _warehouseRepository = warehouseRepository;
        _logger = logger;
        _cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
    }

    public async Task<IReadOnlyList<WarehouseCacheDto>> GetActiveWarehousesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"warehouses:{tenantId}";
        
        try
        {
            var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cachedData))
            {
                var cached = JsonSerializer.Deserialize<List<WarehouseCacheDto>>(cachedData);
                if (cached != null)
                {
                    _logger.LogDebug("Cache hit for warehouses of tenant {TenantId}", tenantId);
                    return cached;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache read failed for tenant {TenantId}, falling back to database", tenantId);
        }

        var warehouses = await _warehouseRepository.GetActiveWarehousesByTenantAsync(tenantId);
        var dtos = warehouses.Select(w => new WarehouseCacheDto(
            w.Id,
            w.Name,
            w.Code,
            w.Latitude,
            w.Longitude,
            w.IsActive
        )).ToList();
        try
        {
            var serialized = JsonSerializer.Serialize(dtos);
            await _cache.SetStringAsync(cacheKey, serialized, _cacheOptions, cancellationToken);
            _logger.LogDebug("Cache updated for warehouses of tenant {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache write failed for tenant {TenantId}", tenantId);
        }

        return dtos;
    }

    public async Task InvalidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"warehouses:{tenantId}";
        await _cache.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogDebug("Cache invalidated for tenant {TenantId}", tenantId);
    }
}