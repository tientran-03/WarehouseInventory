using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public partial class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITenantRepository _tenantRepository;

    public CategoryService(ICategoryRepository categoryRepository, ITenantRepository tenantRepository)
    {
        _categoryRepository = categoryRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task<IEnumerable<CategoryResponse>> GetByTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }

        var categories = await _categoryRepository.GetByTenantAsync(tenantId);
        return categories.Select(c => new CategoryResponse(c.Id, c.TenantId, c.Name, c.Slug));
    }

    public async Task<CategoryResponse> CreateAsync(UpsertCategoryRequest request)
    {
        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new BadRequestException("Tên danh mục không được để trống.");
        }

        var slug = await EnsureUniqueSlugAsync(request.TenantId, ToSlug(name));

        var category = new Category
        {
            TenantId = request.TenantId,
            Name = name,
            Slug = slug,
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return new CategoryResponse(category.Id, category.TenantId, category.Name, category.Slug);
    }

    private async Task<string> EnsureUniqueSlugAsync(Guid tenantId, string baseSlug)
    {
        var categories = await _categoryRepository.GetByTenantAsync(tenantId);
        var existing = categories.Select(c => c.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!existing.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (var i = 2; i <= 99; i++)
        {
            var candidate = $"{baseSlug}-{i}";
            if (!existing.Contains(candidate))
            {
                return candidate;
            }
        }

        throw new BadRequestException("Không thể tạo slug duy nhất cho danh mục.");
    }

    private static string ToSlug(string name)
    {
        var normalized = name.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            builder.Append(ch);
        }

        var slug = builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = slug.Replace('đ', 'd').Replace('Đ', 'd');
        slug = NonAlphaNumericRegex().Replace(slug, "-");
        slug = MultiDashRegex().Replace(slug, "-").Trim('-');

        return string.IsNullOrWhiteSpace(slug) ? "danh-muc" : slug;
    }

    [GeneratedRegex(@"[^a-z0-9]+", RegexOptions.Compiled)]
    private static partial Regex NonAlphaNumericRegex();

    [GeneratedRegex(@"-+", RegexOptions.Compiled)]
    private static partial Regex MultiDashRegex();
}
