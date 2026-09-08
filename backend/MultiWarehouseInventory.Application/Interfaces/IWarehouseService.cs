using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseResponse>> GetAllAsync();
    Task<WarehouseResponse> GetByIdAsync(Guid id);
    Task<WarehouseResponse> CreateAsync(UpsertWarehouseRequest request);
    Task UpdateAsync(Guid id, UpsertWarehouseRequest request);
    Task DeleteAsync(Guid id);
}
