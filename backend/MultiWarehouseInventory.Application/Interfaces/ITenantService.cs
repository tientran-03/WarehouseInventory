using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface ITenantService
{
    Task<IEnumerable<TenantResponse>> GetAllAsync();
    Task<TenantResponse> GetByIdAsync(Guid id);
    Task<TenantResponse> CreateAsync(UpsertTenantRequest request);
    Task AddCompanyAccountAsync(Guid tenantId, CreateCompanyAccountRequest request);
    Task ResendVerificationEmailAsync(Guid tenantId, ResendVerificationEmailRequest request);
    Task UpdateAsync(Guid id, UpsertTenantRequest request);
    Task DeleteAsync(Guid id);
}
