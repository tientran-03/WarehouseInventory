using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductResponse>> GetByTenantAsync(Guid tenantId);
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(UpsertProductRequest request);
    Task UpdateAsync(Guid id, UpsertProductRequest request);
    Task DeleteAsync(Guid id);
    Task UpdateImageUrlAsync(Guid id, string imageUrl);
}

public interface IInventoryService
{
    Task<IEnumerable<InventoryResponse>> GetByTenantAsync(Guid tenantId);
    Task<IEnumerable<InventoryResponse>> GetByWarehouseAsync(Guid warehouseId);
    Task<InventoryResponse> UpsertAsync(UpsertInventoryRequest request);
    Task<InventoryResponse> UpdateAsync(Guid id, UpsertInventoryRequest request);
    Task DeleteAsync(Guid id);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetByTenantAsync(Guid tenantId);
    Task<CategoryResponse> CreateAsync(UpsertCategoryRequest request);
}
