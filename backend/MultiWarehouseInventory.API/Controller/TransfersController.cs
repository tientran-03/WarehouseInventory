using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class TransfersController : ControllerBase
{
    private readonly ITransferService _transferService;

    public TransfersController(ITransferService transferService) => _transferService = transferService;

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId)
    {
        var result = await _transferService.GetByTenantAsync(tenantId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransferRequest request)
    {
        var result = await _transferService.CreateAsync(request);
        return CreatedAtAction(nameof(GetByTenant), new { tenantId = result.TenantId }, result);
    }

    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
    {
        var result = await _transferService.StartAsync(id);
        return Ok(result);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id)
    {
        var result = await _transferService.CompleteAsync(id);
        return Ok(result);
    }
}
