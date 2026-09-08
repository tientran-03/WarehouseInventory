using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;
    private readonly IWarehouseZoneRepository _warehouseZoneRepository;

    public InventoryController(
        IInventoryService inventoryService,
        IWarehouseZoneRepository warehouseZoneRepository)
    {
        _inventoryService = inventoryService;
        _warehouseZoneRepository = warehouseZoneRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        var result = await _inventoryService.GetByTenantAsync(tenantId);
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> GetByWarehouse(Guid warehouseId)
    {
        var result = await _inventoryService.GetByWarehouseAsync(warehouseId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Upsert([FromBody] UpsertInventoryRequest request)
    {
        var result = await _inventoryService.UpsertAsync(request);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertInventoryRequest request)
    {
        var result = await _inventoryService.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _inventoryService.DeleteAsync(id);
        return NoContent();
    }

}
