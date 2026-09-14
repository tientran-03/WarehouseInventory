# Caching Strategy

## Overview

The system uses **Redis Distributed Caching** to improve performance by reducing database load and response times. The caching strategy follows the **Cache-Aside Pattern** (also known as Lazy Loading).

## Cache-Aside Pattern

### How It Works

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Client    │                    │    Redis    │                    │  Database   │
└─────────────┘                    └─────────────┘                    └─────────────┘
       │                                  │                                  │
       │  1. Request data                 │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │                                  │
       │  2. Cache miss                   │                                  │
       │<─────────────────────────────────│                                  │
       │                                  │                                  │
       │  3. Query database               │                                  │
       │──────────────────────────────────────────────────────────────────>│
       │                                  │                                  │
       │  4. Return data                 │                                  │
       │<──────────────────────────────────────────────────────────────────│
       │                                  │                                  │
       │  5. Update cache                │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │                                  │
       │  6. Return data to client        │                                  │
       │<─────────────────────────────────│                                  │
```

### Implementation

```csharp
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
            // 1. Try to get from cache
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

        // 2. Cache miss - query database
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
            // 3. Update cache
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
```

## Cache Keys

### Key Naming Convention

Cache keys follow the pattern: `{resource}:{identifier}`

| Resource | Key Pattern | Example |
|----------|-------------|---------|
| Warehouses | `warehouses:{tenantId}` | `warehouses:a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| Products | `products:{tenantId}` | `products:a1b2c3d4-e5f6-7890-abcd-ef1234567890` |
| Inventory | `inventory:{warehouseId}` | `inventory:b2c3d4e5-f6g7-8901-bcde-f12345678901` |
| User Permissions | `permissions:{userId}` | `permissions:c3d4e5f6-g7h8-9012-cdef-123456789012` |

## Cache Configuration

### TTL (Time To Live)

Different data types have different TTL values:

| Data Type | TTL | Reason |
|-----------|-----|--------|
| Warehouses | 30 minutes | Changes infrequently |
| Products | 1 hour | Catalog changes rarely |
| Inventory | 5 minutes | Changes frequently |
| User Permissions | 15 minutes | Security-related |
| Configuration | 1 hour | Changes rarely |

### Cache Entry Options

```csharp
var cacheOptions = new DistributedCacheEntryOptions
{
    // Absolute expiration - cache expires after fixed time
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
    
    // Sliding expiration - cache expires if not accessed for X time
    SlidingExpiration = TimeSpan.FromMinutes(10),
    
    // Priority for eviction when memory is full
    Priority = CacheItemPriority.Normal
};
```

## Cache Invalidation

### Manual Invalidation

Explicitly invalidate cache when data changes:

```csharp
public async Task<WarehouseResponse> UpdateAsync(Guid id, UpsertWarehouseRequest request)
{
    var warehouse = await _repository.GetByIdAsync(id);
    // ... update logic ...
    
    // Invalidate cache
    await _warehouseCacheService.InvalidateTenantAsync(warehouse.TenantId);
    
    return _mapper.Map<WarehouseResponse>(warehouse);
}
```

### Automatic Invalidation

Use database change tracking or events:

```csharp
public class WarehouseCreatedEventHandler : INotificationHandler<WarehouseCreatedEvent>
{
    private readonly IWarehouseCacheService _cacheService;

    public async Task Handle(WarehouseCreatedEvent notification, CancellationToken cancellationToken)
    {
        await _cacheService.InvalidateTenantAsync(notification.TenantId);
    }
}
```

### Bulk Invalidation

Invalidate all cache for a tenant:

```csharp
public async Task InvalidateAllTenantCacheAsync(Guid tenantId)
{
    var keys = new[]
    {
        $"warehouses:{tenantId}",
        $"products:{tenantId}",
        $"inventory:*:{tenantId}"
    };
    
    foreach (var key in keys)
    {
        await _cache.RemoveAsync(key);
    }
}
```

## Caching Strategies by Use Case

### 1. Read-Heavy Data (Warehouses, Products)

**Strategy**: Cache-Aside with long TTL

```csharp
public async Task<IReadOnlyList<Warehouse>> GetWarehousesAsync(Guid tenantId)
{
    var cacheKey = $"warehouses:{tenantId}";
    var cached = await _cache.GetAsync<List<Warehouse>>(cacheKey);
    
    if (cached != null)
        return cached;
    
    var warehouses = await _repository.GetByTenantAsync(tenantId);
    await _cache.SetAsync(cacheKey, warehouses, TimeSpan.FromHours(1));
    
    return warehouses;
}
```

### 2. Write-Heavy Data (Inventory)

**Strategy**: Short TTL with write-through

```csharp
public async Task UpdateInventoryAsync(Guid inventoryId, int quantity)
{
    var inventory = await _repository.GetByIdAsync(inventoryId);
    inventory.OnHandStock = quantity;
    await _repository.UpdateAsync(inventory);
    
    // Invalidate cache immediately
    var cacheKey = $"inventory:{inventory.WarehouseId}";
    await _cache.RemoveAsync(cacheKey);
}
```

### 3. Computed Data (Analytics)

**Strategy**: Cache computed results

```csharp
public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync(Guid tenantId)
{
    var cacheKey = $"analytics:dashboard:{tenantId}";
    var cached = await _cache.GetAsync<DashboardAnalytics>(cacheKey);
    
    if (cached != null)
        return cached;
    
    var analytics = await ComputeAnalyticsAsync(tenantId);
    await _cache.SetAsync(cacheKey, analytics, TimeSpan.FromMinutes(15));
    
    return analytics;
}
```

## Redis Configuration

### Connection String

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  }
}
```

### Production Configuration

```json
{
  "ConnectionStrings": {
    "Redis": "redis-server:6379,password=your_password,ssl=true"
  }
}
```

### Dependency Injection

```csharp
// Program.cs
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "WarehouseInventory";
});
```

## Performance Metrics

### Cache Hit Ratio

Monitor cache effectiveness:

```csharp
public class CacheMetrics
{
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    
    public double HitRatio => CacheHits / (double)(CacheHits + CacheMisses);
}
```

Target hit ratio: > 80%

### Response Time Comparison

| Operation | Without Cache | With Cache | Improvement |
|-----------|---------------|------------|-------------|
| Get Warehouses | 150ms | 5ms | 97% faster |
| Get Products | 200ms | 8ms | 96% faster |
| Get Inventory | 100ms | 3ms | 97% faster |

## Cache Warming

### Preload Cache on Startup

```csharp
public class CacheWarmupService : IHostedService
{
    private readonly IWarehouseCacheService _cacheService;
    private readonly ITenantRepository _tenantRepository;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var tenants = await _tenantRepository.GetAllActiveAsync();
        
        foreach (var tenant in tenants)
        {
            await _cacheService.GetActiveWarehousesAsync(tenant.Id, cancellationToken);
        }
    }
}
```

## Distributed Caching Considerations

### Cache Stampede

Multiple requests miss cache simultaneously and query database:

**Solution**: Use cache locking or short TTL with jitter:

```csharp
public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory)
{
    var cached = await _cache.GetAsync<T>(key);
    if (cached != null)
        return cached;
    
    // Add random jitter to prevent stampede
    var ttl = TimeSpan.FromMinutes(30) + TimeSpan.FromSeconds(Random.Shared.Next(0, 60));
    
    var value = await factory();
    await _cache.SetAsync(key, value, ttl);
    
    return value;
}
```

### Cache Coherency

Ensure cache consistency across multiple instances:

**Solution**: Use pub/sub for cache invalidation:

```csharp
public class CacheInvalidationPublisher
{
    private readonly ISubscriber _subscriber;
    private readonly IPublisher _publisher;

    public async Task PublishInvalidationAsync(string key)
    {
        await _publisher.PublishAsync("cache-invalidation", key);
    }

    public void SubscribeToInvalidation()
    {
        _subscriber.Subscribe("cache-invalidation", (channel, message) =>
        {
            var key = message.ToString();
            _cache.RemoveAsync(key);
        });
    }
}
```

## Monitoring

### Cache Statistics

```csharp
public class CacheMonitor
{
    public async Task LogCacheStatisticsAsync()
    {
        var info = await _cache.GetStatsAsync();
        _logger.LogInformation("Cache Stats: {Stats}", info);
    }
}
```

### Health Check

```csharp
public class RedisHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.SetStringAsync("health-check", "ok", cancellationToken);
            await _cache.RemoveAsync("health-check", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```

## Best Practices

### DO ✅
- Use appropriate TTL for each data type
- Invalidate cache on data changes
- Monitor cache hit ratio
- Use cache warming for critical data
- Handle cache failures gracefully
- Use compression for large cached objects

### DON'T ❌
- Cache sensitive data without encryption
- Use very long TTL for frequently changing data
- Forget to invalidate cache on updates
- Cache entire database tables
- Ignore cache failures (fallback to database)
- Use cache as primary data store

## Troubleshooting

### Cache Not Working

1. Check Redis connection:
```bash
redis-cli ping
```

2. Verify connection string in appsettings.json

3. Check Redis logs for errors

4. Test cache manually:
```csharp
await _cache.SetStringAsync("test", "value");
var value = await _cache.GetStringAsync("test");
```

### High Cache Miss Rate

1. Review TTL values (may be too short)
2. Check cache invalidation logic (may be too aggressive)
3. Monitor cache size (may be evicted due to memory pressure)

### Memory Issues

1. Monitor Redis memory usage:
```bash
redis-cli info memory
```

2. Set max memory policy:
```bash
redis-cli CONFIG SET maxmemory 1gb
redis-cli CONFIG SET maxmemory-policy allkeys-lru
```

3. Use compression for large objects:
```csharp
var compressed = CompressionHelper.Compress(data);
await _cache.SetAsync(key, compressed);
```

## Additional Resources

- [Redis Documentation](https://redis.io/documentation)
- [Microsoft Distributed Cache](https://docs.microsoft.com/aspnet/core/performance/caching/distributed)
- [Cache-Aside Pattern](https://docs.microsoft.com/azure/architecture/patterns/cache-aside)
