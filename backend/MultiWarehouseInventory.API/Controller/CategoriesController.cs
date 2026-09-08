using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        var result = await _categoryService.GetByTenantAsync(tenantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertCategoryRequest request)
    {
        var result = await _categoryService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = result.TenantId }, result);
    }
}
