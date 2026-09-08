using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Enums;
using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.API.Controller;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly IOrderAllocationService _orderAllocationService;
    private readonly IOrderService _orderService;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrderAllocationService orderAllocationService,
        IOrderService orderService,
        IValidator<CreateOrderRequest> validator,
        ILogger<OrdersController> logger)
    {
        _orderAllocationService = orderAllocationService;
        _orderService = orderService;
        _validator = validator;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetByTenant([FromQuery] Guid tenantId, [FromQuery] string? status = null)
    {
        var result = await _orderService.GetByTenantAsync(tenantId, status);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _orderService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)) && 
            !User.IsInRole(nameof(UserRole.Manager)) && 
            !User.IsInRole(nameof(UserRole.Staff)))
        {
            return Forbid();
        }

        var result = await _orderService.UpdateStatusAsync(id, request.Status);
        return Ok(result);
    }

    [HttpPost("{id:guid}/create-stock-documents")]
    public async Task<IActionResult> CreateStockDocumentsFromOrder(Guid id)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)) && 
            !User.IsInRole(nameof(UserRole.Manager)) && 
            !User.IsInRole(nameof(UserRole.Staff)))
        {
            return Forbid();
        }

        if (!Guid.TryParse(User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized(new { success = false, errorCode = "UNAUTHORIZED", message = "Phiên đăng nhập không hợp lệ." });
        }

        var result = await _orderService.CreateStockDocumentsFromOrderAsync(id, userId);
        return Ok(result);
    }
    [HttpPost("webhook")]
    [ProducesResponseType(typeof(OrderAllocationResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReceiveWebhook(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "X-Webhook-Source")] string? source = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Webhook received from {Source} for order {OrderCode}",
            source ?? "unknown",
            request.OrderCode);

        await ValidateRequestAsync(request, cancellationToken);
        var result = await _orderAllocationService.ProcessIncomingOrderAsync(request, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderAllocationResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        await ValidateRequestAsync(request, cancellationToken);
        var result = await _orderAllocationService.ProcessIncomingOrderAsync(request, cancellationToken);
        return Ok(new { success = true, data = result });
    }

    private async Task ValidateRequestAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            throw new BadRequestException(string.Join("; ", validation.Errors.Select(e => e.ErrorMessage)));
        }
    }
}
