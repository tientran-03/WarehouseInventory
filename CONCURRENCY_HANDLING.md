# Concurrency Handling

## Overview

The system implements several strategies to handle concurrent operations and ensure data consistency in a multi-user environment. This document explains the concurrency control mechanisms used throughout the application.

## Concurrency Challenges

### Common Scenarios

1. **Race Conditions**: Multiple users updating the same data simultaneously
2. **Lost Updates**: One user's changes overwriting another's
3. **Deadlocks**: Multiple operations waiting for each other
4. **Dirty Reads**: Reading uncommitted data
5. **Phantom Reads**: Data appearing/disappearing between reads

## Concurrency Control Strategies

### 1. Database-Level Concurrency

#### Optimistic Concurrency

Used for most operations where conflicts are rare:

```csharp
public class WarehouseInventory : BaseEntity
{
    public Guid Id { get; set; }
    public int OnHandStock { get; set; }
    public int ReservedStock { get; set; }
    public byte[] Version { get; set; } // Concurrency token
}

// On update
var inventory = await _repository.GetByIdAsync(id);
inventory.OnHandStock = newQuantity;

if (_context.Entry(inventory).State == EntityState.Modified)
{
    _context.Entry(inventory).OriginalValues["Version"] = originalVersion;
}

await _context.SaveChangesAsync();
```

#### Pessimistic Concurrency

Used for critical operations where conflicts are likely:

```csharp
public async Task UpdateInventoryWithLockAsync(Guid inventoryId, int quantity)
{
    using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    
    try
    {
        var inventory = await _context.WarehouseInventories
            .Where(i => i.Id == inventoryId)
            .FirstOrDefaultAsync();
        
        if (inventory == null)
            throw new NotFoundException(nameof(WarehouseInventory), inventoryId);
        
        inventory.OnHandStock = quantity;
        await _context.SaveChangesAsync();
        
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### 2. Application-Level Concurrency

#### Stock Reservation System

Prevents overselling by reserving stock for orders:

```csharp
public class StockReservation
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}

public async Task ReserveStockAsync(Guid orderId, List<AllocationLine> allocations)
{
    foreach (var allocation in allocations)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(
            allocation.WarehouseId, 
            allocation.ProductId);
        
        if (inventory.AvailableStock < allocation.Quantity)
            throw new InsufficientStockException(...);
        
        // Reserve stock
        inventory.ReservedStock += allocation.Quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        
        // Create reservation record
        var reservation = new StockReservation
        {
            OrderId = orderId,
            WarehouseId = allocation.WarehouseId,
            ProductId = allocation.ProductId,
            Quantity = allocation.Quantity
        };
        await _reservationRepository.AddAsync(reservation);
    }
    
    await _unitOfWork.SaveChangesAsync();
}
```

#### Automatic Release

Release reservations when orders are cancelled or completed:

```csharp
public async Task ReleaseStockReservationsAsync(Order order)
{
    var reservations = await _reservationRepository.GetByOrderIdAsync(order.Id);
    
    foreach (var reservation in reservations)
    {
        var inventory = await _inventoryRepository.GetByIdAsync(
            reservation.WarehouseId,
            reservation.ProductId);
        
        inventory.ReservedStock -= reservation.Quantity;
        await _inventoryRepository.UpdateAsync(inventory);
        
        reservation.ReleasedAt = DateTime.UtcNow;
        await _reservationRepository.UpdateAsync(reservation);
    }
    
    await _unitOfWork.SaveChangesAsync();
}
```

### 3. Transaction Management

#### Unit of Work Pattern

Ensures atomic operations:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
```

#### Transaction Scopes

```csharp
public async Task CreateStockDocumentAsync(CreateStockDocumentRequest request)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Create stock document
        var document = new StockDocument { ... };
        await _documentRepository.AddAsync(document);
        
        // Update inventory
        foreach (var line in request.Lines)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(
                line.WarehouseZoneId,
                line.ProductId);
            
            if (document.Type == "Inbound")
                inventory.OnHandStock += line.Quantity;
            else
                inventory.OnHandStock -= line.Quantity;
            
            await _inventoryRepository.UpdateAsync(inventory);
            
            // Create stock movement
            var movement = new StockMovement { ... };
            await _movementRepository.AddAsync(movement);
        }
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

### 4. SignalR Concurrency

#### Connection Management

Handle multiple connections per user:

```csharp
public class WarehouseOrdersHub : Hub
{
    private static readonly ConcurrentDictionary<string, HashSet<string>> UserConnections = 
        new ConcurrentDictionary<string, HashSet<string>>();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        UserConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> { connectionId },
            (_, connections) => { connections.Add(connectionId); return connections; });
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        var connectionId = Context.ConnectionId;
        
        if (UserConnections.TryGetValue(userId, out var connections))
        {
            connections.Remove(connectionId);
            if (connections.Count == 0)
                UserConnections.TryRemove(userId, out _);
        }
        
        await base.OnDisconnectedAsync(exception);
    }
}
```

#### Message Ordering

Ensure messages are processed in order:

```csharp
public class OrderedMessageQueue
{
    private readonly ConcurrentQueue<OrderedMessage> _queue = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task EnqueueAsync(OrderedMessage message)
    {
        await _semaphore.WaitAsync();
        try
        {
            _queue.Enqueue(message);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<OrderedMessage?> DequeueAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _queue.TryDequeue(out var message);
            return message;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### 5. Cache Concurrency

#### Cache Stampede Prevention

Use locking to prevent multiple cache misses:

```csharp
public class CacheLockManager
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = 
        new ConcurrentDictionary<string, SemaphoreSlim>();

    public async Task<T> GetOrAddAsync<T>(string key, Func<Task<T>> factory)
    {
        var lockKey = $"lock:{key}";
        var semaphore = _locks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        
        await semaphore.WaitAsync();
        try
        {
            // Double-check pattern
            var cached = await _cache.GetAsync<T>(key);
            if (cached != null)
                return cached;
            
            var value = await factory();
            await _cache.SetAsync(key, value);
            return value;
        }
        finally
        {
            semaphore.Release();
            _locks.TryRemove(lockKey, out _);
        }
    }
}
```

#### Cache Invalidation

Broadcast cache invalidation across instances:

```csharp
public class CacheInvalidationService
{
    private readonly ISubscriber _subscriber;
    private readonly IPublisher _publisher;

    public CacheInvalidationService(IConnectionMultiplexer redis)
    {
        _subscriber = redis.GetSubscriber();
        _publisher = redis.GetSubscriber();
        
        _subscriber.Subscribe("cache-invalidation", async (channel, message) =>
        {
            var key = message.ToString();
            await _cache.RemoveAsync(key);
        });
    }

    public async Task InvalidateAsync(string key)
    {
        await _cache.RemoveAsync(key);
        await _publisher.PublishAsync("cache-invalidation", key);
    }
}
```

## Isolation Levels

### Read Committed (Default)

```csharp
await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
```

- Prevents dirty reads
- Allows non-repeatable reads
- Allows phantom reads

### Repeatable Read

```csharp
await _context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
```

- Prevents dirty reads
- Prevents non-repeatable reads
- Allows phantom reads

### Serializable

```csharp
await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
```

- Prevents dirty reads
- Prevents non-repeatable reads
- Prevents phantom reads
- Highest isolation, lowest concurrency

## Deadlock Prevention

### Consistent Lock Ordering

Always acquire locks in the same order:

```csharp
// Bad - can cause deadlock
async Task TransferStock(Guid fromWarehouse, Guid toWarehouse, Guid product)
{
    await LockWarehouseAsync(fromWarehouse);
    await LockWarehouseAsync(toWarehouse);
    // ... transfer logic ...
}

// Good - consistent order
async Task TransferStock(Guid warehouse1, Guid warehouse2, Guid product)
{
    var ordered = new[] { warehouse1, warehouse2 }.OrderBy(x => x);
    await LockWarehouseAsync(ordered.First());
    await LockWarehouseAsync(ordered.Last());
    // ... transfer logic ...
}
```

### Timeout Handling

Set transaction timeouts:

```csharp
var transaction = await _context.Database.BeginTransactionAsync(
    IsolationLevel.ReadCommitted,
    new TransactionOptions { Timeout = TimeSpan.FromSeconds(30) });
```

### Retry Logic

Implement exponential backoff for transient failures:

```csharp
public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            return await operation();
        }
        catch (DbUpdateConcurrencyException ex) when (attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
            await Task.Delay(delay);
        }
    }
    
    throw new InvalidOperationException("Max retries exceeded");
}
```

## Concurrency in Specific Scenarios

### Order Allocation

```csharp
public async Task<OrderDetailDto> AllocateOrderAsync(Guid orderId)
{
    using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
    
    try
    {
        var order = await _repository.GetByIdWithDetailsAsync(orderId);
        
        // Check if already allocated
        if (order.Status != "Pending")
            throw new BadRequestException("Order already allocated");
        
        // Get current inventory with lock
        var inventories = await _inventoryRepository.GetByTenantAsync(order.TenantId);
        
        // Allocate with stock check
        var allocations = OrderAllocationPlanner.Plan(
            order.Items,
            warehouses,
            inventories,
            order.CustomerLatitude,
            order.CustomerLongitude);
        
        // Reserve stock
        foreach (var allocation in allocations)
        {
            var inventory = inventories.First(i => 
                i.WarehouseId == allocation.WarehouseId && 
                i.ProductId == allocation.ProductId);
            
            if (inventory.AvailableStock < allocation.Quantity)
                throw new InsufficientStockException(...);
            
            inventory.ReservedStock += allocation.Quantity;
            await _inventoryRepository.UpdateAsync(inventory);
        }
        
        // Update order status
        order.Status = "Allocated";
        await _repository.UpdateAsync(order);
        
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        
        return MapToDetail(order);
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### Stock Document Creation

```csharp
public async Task<StockDocumentDetailResponse> CreateAsync(
    CreateStockDocumentRequest request, 
    Guid createdBy)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        // Validate stock availability for outbound
        if (request.Type == "Outbound")
        {
            foreach (var line in request.Lines)
            {
                var inventory = await _inventoryRepository.GetByIdAsync(
                    line.WarehouseZoneId,
                    line.ProductId);
                
                if (inventory.OnHandStock < line.Quantity)
                    throw new InsufficientStockException(...);
            }
        }
        
        // Create document
        var document = new StockDocument { ... };
        await _documentRepository.AddAsync(document);
        
        // Update inventory and create movements
        foreach (var line in request.Lines)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(
                line.WarehouseZoneId,
                line.ProductId);
            
            if (request.Type == "Inbound")
                inventory.OnHandStock += line.Quantity;
            else
                inventory.OnHandStock -= line.Quantity;
            
            await _inventoryRepository.UpdateAsync(inventory);
            
            var movement = new StockMovement { ... };
            await _movementRepository.AddAsync(movement);
        }
        
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();
        
        return _mapper.Map<StockDocumentDetailResponse>(document);
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## Monitoring Concurrency

### Deadlock Detection

```csharp
public class DeadlockMonitor
{
    private readonly ILogger<DeadlockMonitor> _logger;

    public async Task MonitorDeadlocksAsync()
    {
        var deadlocks = await _context.Database
            .SqlQueryRaw<DeadlockInfo>("SHOW ENGINE INNODB STATUS")
            .ToListAsync();
        
        foreach (var deadlock in deadlocks)
        {
            _logger.LogError("Deadlock detected: {DeadlockInfo}", deadlock);
        }
    }
}
```

### Lock Wait Statistics

```csharp
public async Task LogLockStatisticsAsync()
{
    var stats = await _context.Database
        .SqlQueryRaw<LockStats>("SELECT * FROM information_schema.INNODB_LOCKS")
        .ToListAsync();
    
    _logger.LogInformation("Lock statistics: {Stats}", stats);
}
```

## Best Practices

### DO ✅
- Use appropriate isolation levels for each operation
- Implement retry logic for transient failures
- Use transactions for multi-step operations
- Reserve stock for orders to prevent overselling
- Monitor deadlock occurrences
- Keep transactions as short as possible
- Release locks in reverse order of acquisition

### DON'T ❌
- Use Serializable isolation level unnecessarily
- Hold locks for extended periods
- Forget to rollback on exceptions
- Ignore concurrency exceptions
- Nest transactions deeply
- Use long-running transactions
- Forget to release locks

## Testing Concurrency

### Unit Tests

```csharp
[Fact]
public async Task ConcurrentStockUpdate_ShouldHandleRaceCondition()
{
    // Arrange
    var inventory = await CreateInventoryAsync(100);
    var tasks = new List<Task>();
    
    // Act - 10 concurrent updates
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            await _service.UpdateStockAsync(inventory.Id, -10);
        }));
    }
    
    await Task.WhenAll(tasks);
    
    // Assert
    var finalInventory = await _repository.GetByIdAsync(inventory.Id);
    finalInventory.OnHandStock.Should().Be(0);
}
```

### Load Tests

```csharp
[Fact]
public async Task HighConcurrency_ShouldNotCauseDeadlocks()
{
    // Arrange
    var tasks = new List<Task>();
    
    // Act - 100 concurrent operations
    for (int i = 0; i < 100; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            await _service.CreateOrderAsync(CreateRandomOrder());
        }));
    }
    
    // Assert - should complete without deadlock
    await Task.WhenAll(tasks);
}
```

## Troubleshooting

### Deadlocks

1. **Identify deadlocks**: Check database logs
2. **Analyze deadlock graph**: Use database tools
3. **Fix lock ordering**: Ensure consistent order
4. **Reduce transaction scope**: Shorten transactions
5. **Add retry logic**: Handle deadlocks gracefully

### Lock Timeouts

1. **Check lock wait time**: Monitor lock statistics
2. **Optimize queries**: Reduce query execution time
3. **Add indexes**: Speed up data access
4. **Review isolation level**: Use lower isolation if possible

### High Contention

1. **Identify hotspots**: Monitor frequently accessed data
2. **Add caching**: Reduce database load
3. **Partition data**: Split large tables
4. **Queue operations**: Serialize conflicting operations

## Additional Resources

- [EF Core Concurrency](https://docs.microsoft.com/ef/core/saving/concurrency)
- [Database Isolation Levels](https://en.wikipedia.org/wiki/Isolation_(database_systems))
- [Redis Concurrency](https://redis.io/docs/manual/patterns/distributed-locks/)
- [SignalR Scaling](https://docs.microsoft.com/aspnet/core/signalr/scale)
