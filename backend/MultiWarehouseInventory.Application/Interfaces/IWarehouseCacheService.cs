using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IWarehouseCacheService
{
    Task<IReadOnlyList<WarehouseCacheDto>> GetActiveWarehousesAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);

    Task InvalidateTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
