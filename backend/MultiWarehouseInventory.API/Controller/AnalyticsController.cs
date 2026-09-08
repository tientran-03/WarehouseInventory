using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.Interfaces;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService) => _analyticsService = analyticsService;

    [HttpGet("financials")]
    public async Task<IActionResult> GetFinancials([FromQuery] Guid tenantId)
    {
        var result = await _analyticsService.GetFinancialSummaryAsync(tenantId);
        return Ok(result);
    }

    [HttpGet("balancing")]
    public async Task<IActionResult> GetBalancing([FromQuery] Guid tenantId)
    {
        var result = await _analyticsService.GetBalancingSuggestionsAsync(tenantId);
        return Ok(result);
    }
}
