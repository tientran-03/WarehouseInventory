using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class WarehouseZonesController : ControllerBase
{
    private readonly IWarehouseZoneService _zoneService;

    public WarehouseZonesController(IWarehouseZoneService zoneService) => _zoneService = zoneService;

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        var result = await _zoneService.GetByTenantAsync(tenantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateZoneRequest request)
    {
        var result = await _zoneService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = result.TenantId }, result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _zoneService.DeleteAsync(id);
        return NoContent();
    }
}
