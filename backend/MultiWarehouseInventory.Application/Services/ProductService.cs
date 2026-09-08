using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        ITenantRepository tenantRepository,
        ICategoryRepository categoryRepository,
        IMapper mapper)
    {
        _productRepository = productRepository;
        _tenantRepository = tenantRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantExistsAsync(tenantId);
        var products = await _productRepository.GetByTenantAsync(tenantId);
        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(UpsertProductRequest request)
    {
        await EnsureTenantExistsAsync(request.TenantId);
        await EnsureCategoryExistsAsync(request.CategoryId, request.TenantId);

        if (await _productRepository.GetBySkuAsync(request.Sku, request.TenantId) is not null)
        {
            throw new BadRequestException($"SKU '{request.Sku}' đã tồn tại trong tenant này.");
        }

        var product = _mapper.Map<Product>(request);
        product.IsActive = true;

        await _productRepository.AddAsync(product);
        await _productRepository.SaveChangesAsync();

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task UpdateAsync(Guid id, UpsertProductRequest request)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        await EnsureCategoryExistsAsync(request.CategoryId, request.TenantId);

        var existingSku = await _productRepository.GetBySkuAsync(request.Sku, request.TenantId);
        if (existingSku is not null && existingSku.Id != id)
        {
            throw new BadRequestException($"SKU '{request.Sku}' đã được sử dụng bởi sản phẩm khác.");
        }

        _mapper.Map(request, product);
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();
    }

    public async Task UpdateImageUrlAsync(Guid id, string imageUrl)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null || !product.IsActive)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        product.ImageUrl = imageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepository.UpdateAsync(product);
        await _productRepository.SaveChangesAsync();
    }

    private async Task EnsureTenantExistsAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private async Task EnsureCategoryExistsAsync(Guid categoryId, Guid tenantId)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category is null || category.TenantId != tenantId)
        {
            throw new NotFoundException(nameof(Category), categoryId);
        }
    }
}
