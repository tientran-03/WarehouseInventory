using MultiWarehouseInventory.Domain.Events;

namespace MultiWarehouseInventory.Domain.Interfaces;

/// <summary>
/// Interface để dispatch domain events đến handlers
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}