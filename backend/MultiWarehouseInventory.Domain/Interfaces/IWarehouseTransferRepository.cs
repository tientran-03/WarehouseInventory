using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IWarehouseTransferRepository
{
    Task<List<WarehouseTransfer>> GetByTenantAsync(Guid tenantId);
    Task<WarehouseTransfer?> GetByIdAsync(Guid id);
    Task AddAsync(WarehouseTransfer transfer);
    Task UpdateAsync(WarehouseTransfer transfer);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
