using MultiWarehouseInventory.Application.DTOs;

namespace MultiWarehouseInventory.Application.Interfaces;

public interface ITransferService
{
    Task<IEnumerable<TransferResponse>> GetByTenantAsync(Guid tenantId);
    Task<TransferResponse> CreateAsync(CreateTransferRequest request);
    Task<TransferResponse> StartAsync(Guid id);
    Task<TransferResponse> CompleteAsync(Guid id);
}

public interface IStocktakeService
{
    Task<IEnumerable<StocktakeResponse>> GetByTenantAsync(Guid tenantId);
    Task<StocktakeResponse> CreateAsync(CreateStocktakeRequest request);
    Task<StocktakeResponse> CompleteAsync(Guid id, CompleteStocktakeRequest request);
}

public interface IWarehouseZoneService
{
    Task<IEnumerable<ZoneResponse>> GetByTenantAsync(Guid tenantId);
    Task<ZoneResponse> CreateAsync(CreateZoneRequest request);
    Task DeleteAsync(Guid id);
}



public interface IAnalyticsService
{
    Task<FinancialSummaryResponse> GetFinancialSummaryAsync(Guid tenantId);
    Task<IEnumerable<BalancingSuggestionResponse>> GetBalancingSuggestionsAsync(Guid tenantId);
}
