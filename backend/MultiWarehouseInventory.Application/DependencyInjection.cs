using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Application.Services;

namespace MultiWarehouseInventory.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(DependencyInjection).Assembly);
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IOrderAllocationService, OrderAllocationService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IStocktakeService, StocktakeService>();
        services.AddScoped<IWarehouseZoneService, WarehouseZoneService>();
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IStockDocumentService, StockDocumentService>();
        services.AddScoped<IInvoiceService, InvoiceService>();

        return services;
    }
}
