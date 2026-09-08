using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class StocktakesController : ControllerBase
{
    private readonly IStocktakeService _stocktakeService;

    public StocktakesController(IStocktakeService stocktakeService) => _stocktakeService = stocktakeService;

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        var result = await _stocktakeService.GetByTenantAsync(tenantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStocktakeRequest request)
    {
        var result = await _stocktakeService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = result.TenantId }, result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteStocktakeRequest request)
    {
        var result = await _stocktakeService.CompleteAsync(id, request);
        return Ok(result);
    }
}
