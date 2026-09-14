# Architecture

## Overview

The Multi-Warehouse Inventory Management System follows **Clean Architecture (Domain-Driven Design)** principles with clear separation of concerns across four main layers:

```
┌─────────────────────────────────────────────────────────────┐
│                        Presentation Layer                   │
│                    (API Controllers + Frontend)              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      Application Layer                       │
│              (Services, DTOs, Validation, Mapping)           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                       Domain Layer                           │
│              (Entities, Value Objects, Interfaces)           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                       │
│          (Data Access, External Services, Caching)          │
└─────────────────────────────────────────────────────────────┘
```

## Layer Responsibilities

### 1. Domain Layer (`MultiWarehouseInventory.Domain`)
**Purpose**: Core business logic and enterprise rules

**Contains**:
- **Entities**: `Order`, `Warehouse`, `Product`, `WarehouseInventory`, `StockDocument`, etc.
- **Value Objects**: `UserRole`, `OrderStatus`
- **Interfaces**: `IOrderRepository`, `IWarehouseRepository`, etc.
- **Exceptions**: `NotFoundException`, `BadRequestException`, `InsufficientStockException`
- **Common**: `DistanceCalculator`, `Enums`

**Dependencies**: None (dependency-free)

### 2. Application Layer (`MultiWarehouseInventory.Application`)
**Purpose**: Application-specific business logic and orchestration

**Contains**:
- **Services**: `OrderService`, `WarehouseService`, `StockDocumentService`, etc.
- **DTOs**: Request/Response objects for API
- **Validation**: FluentValidation rules
- **Mapping**: AutoMapper profiles
- **Interfaces**: Application-specific service interfaces

**Dependencies**: Domain Layer

### 3. Infrastructure Layer (`MultiWarehouseInventory.Infrastructure`)
**Purpose**: External concerns and data access implementation

**Contains**:
- **Repositories**: Concrete implementations of domain interfaces
- **Data Access**: `AppDbContext`, Entity Framework configuration
- **External Services**: `WarehouseCacheService` (Redis), Email service
- **Dependency Injection**: Service registration

**Dependencies**: Domain Layer, Application Layer

### 4. API Layer (`MultiWarehouseInventory.API`)
**Purpose**: HTTP API endpoints and presentation

**Contains**:
- **Controllers**: RESTful API endpoints
- **Middleware**: JWT authentication, CORS, exception handling
- **Configuration**: `appsettings.json`, startup configuration
- **Swagger**: API documentation

**Dependencies**: Application Layer, Infrastructure Layer

## Design Patterns

### Repository Pattern
Provides abstraction over data access:

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly AppDbContext _context;
    // Implementation...
}
```

### Unit of Work Pattern
Ensures transactional consistency:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Dependency Injection
Service lifetime management:

```csharp
// Scoped - per HTTP request
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IOrderService, OrderService>();

// Singleton - single instance
services.AddSingleton<IWarehouseCacheService, WarehouseCacheService>();

// Transient - new instance each time
services.AddTransient<ILoggerFactory, LoggerFactory>();
```

### Cache-Aside Pattern
Redis caching implementation:

```csharp
public async Task<IReadOnlyList<WarehouseCacheDto>> GetActiveWarehousesAsync(Guid tenantId)
{
    var cacheKey = $"warehouses:{tenantId}";
    
    // 1. Try cache
    var cached = await _cache.GetStringAsync(cacheKey);
    if (cached != null) return Deserialize(cached);
    
    // 2. Cache miss - query database
    var warehouses = await _repository.GetActiveWarehousesByTenantAsync(tenantId);
    
    // 3. Update cache
    await _cache.SetStringAsync(cacheKey, Serialize(warehouses), _cacheOptions);
    
    return warehouses;
}
```

### Strategy Pattern
Pluggable order allocation strategies:

```csharp
public static class OrderAllocationPlanner
{
    public static IReadOnlyList<AllocationLine> Plan(
        IReadOnlyList<OrderItemDto> orderItems,
        IReadOnlyList<WarehouseCacheDto> warehouses,
        // ... other parameters
    )
    {
        // Allocation logic can be swapped with different strategies
    }
}
```

### Observer Pattern
SignalR for real-time notifications:

```csharp
public class WarehouseOrdersHub : Hub
{
    public async Task NotifyOrderStatusUpdated(OrderDetailDto order)
    {
        await Clients.All.SendAsync("OrderStatusUpdated", order);
    }
}
```

## Data Flow

### Order Allocation Flow

```
Client Request
    │
    ▼
OrdersController.AllocateOrder()
    │
    ▼
OrderAllocationService.AllocateOrderAsync()
    │
    ├─► Get warehouses from cache (Redis)
    │
    ├─► Get inventory from database
    │
    ├─► Calculate distances (Haversine formula)
    │
    ├─► Run allocation algorithm
    │
    ├─► Create stock reservations
    │
    └─► Return allocation result
```

### Stock Document Creation Flow

```
Client Request
    │
    ▼
StockDocumentsController.Create()
    │
    ▼
StockDocumentService.CreateAsync()
    │
    ├─► Validate request
    │
    ├─► For Inbound: Increase inventory
    │
    ├─► For Outbound: Decrease inventory
    │
    ├─► Create stock movements
    │
    ├─► Save transaction (Unit of Work)
    │
    └─► Invalidate cache
```

## Security Architecture

### Authentication Flow
1. Client sends credentials to `/auth/login`
2. Server validates credentials
3. Server generates JWT token
4. Client stores token in localStorage
5. Client includes token in Authorization header
6. Server validates token on each request

### Authorization
- **Role-Based Access Control (RBAC)**: Admin, Manager, Staff
- **Policy-Based Authorization**: Custom policies for specific operations
- **Resource-Based Authorization**: Tenant-specific data access

## Performance Optimization

### Caching Strategy
- **Redis Distributed Cache**: Warehouse data, tenant configurations
- **Cache TTL**: 30 minutes for warehouse data
- **Cache Invalidation**: On warehouse create/update/delete

### Database Optimization
- **Indexing**: Foreign keys, frequently queried columns
- **Eager Loading**: Include related entities to avoid N+1 queries
- **Projection**: Select only required columns
- **Async Operations**: Non-blocking I/O throughout

## Scalability Considerations

### Horizontal Scaling
- Stateless API design
- Distributed caching with Redis
- Database connection pooling
- Load balancer support

### Vertical Scaling
- Multi-core CPU utilization
- Memory optimization
- Efficient query execution

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Domain | .NET 8.0 |
| Application | .NET 8.0, AutoMapper, FluentValidation |
| Infrastructure | EF Core 9.0, MySQL, Redis, SignalR |
| API | ASP.NET Core Web API, JWT, Swagger |
| Frontend | React 19, TypeScript, Ant Design, TailwindCSS |
