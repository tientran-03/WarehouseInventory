using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IStockDocumentService _stockDocumentService;
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly IStockReservationRepository _stockReservationRepository;

    private static readonly string[] ValidStatuses = { "Pending", "Allocated", "Processing", "Shipped", "Delivered", "Cancelled" };

    public OrderService(
        IOrderRepository orderRepository,
        ITenantRepository tenantRepository,
        IStockDocumentService stockDocumentService,
        IWarehouseInventoryRepository inventoryRepository,
        IStockReservationRepository stockReservationRepository)
    {
        _orderRepository = orderRepository;
        _tenantRepository = tenantRepository;
        _stockDocumentService = stockDocumentService;
        _inventoryRepository = inventoryRepository;
        _stockReservationRepository = stockReservationRepository;
    }

    public async Task<IEnumerable<OrderListItemDto>> GetByTenantAsync(Guid tenantId, string? status = null)
    {
        await EnsureTenantExistsAsync(tenantId);

        var orders = await _orderRepository.GetByTenantAsync(tenantId, status);
        return orders.Select(MapToListItem);
    }

    public async Task<OrderDetailDto> GetByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null)
        {
            throw new NotFoundException(nameof(Order), id);
        }

        return MapToDetail(order);
    }

    public async Task<OrderDetailDto> UpdateStatusAsync(Guid id, string status)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new BadRequestException($"Trạng thái không hợp lệ. Các trạng thái hợp lệ: {string.Join(", ", ValidStatuses)}");
        }

        var order = await _orderRepository.GetByIdWithDetailsAsync(id);
        if (order is null)
        {
            throw new NotFoundException(nameof(Order), id);
        }

        if (order.Status == "Cancelled" && status != "Cancelled")
        {
            throw new BadRequestException("Không thể thay đổi trạng thái của đơn hàng đã hủy.");
        }

        if (order.Status == "Delivered" && status != "Delivered")
        {
            throw new BadRequestException("Không thể thay đổi trạng thái của đơn hàng đã giao.");
        }
        if (status == "Cancelled" && order.Status != "Cancelled")
        {
            await ReleaseStockReservationsAsync(order);
        }

        order.Status = status;
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return MapToDetail(order);
    }

    private async Task ReleaseStockReservationsAsync(Order order)
    {
        var reservations = await _stockReservationRepository.GetByOrderIdAsync(order.Id);
        
        foreach (var reservation in reservations)
        {
            var inventories = await _inventoryRepository.GetByWarehouseAsync(reservation.WarehouseId);
            var inventory = inventories.FirstOrDefault(i => i.ProductId == reservation.ProductId);
            
            if (inventory != null && inventory.ReservedStock >= reservation.Quantity)
            {
                inventory.ReservedStock -= reservation.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
            }
            
            await _stockReservationRepository.DeleteAsync(reservation);
        }
        
        await _stockReservationRepository.SaveChangesAsync();
    }

    public async Task<OrderDetailDto> CreateStockDocumentsFromOrderAsync(Guid orderId, Guid createdBy)
    {
        var order = await _orderRepository.GetByIdWithDetailsAsync(orderId);
        if (order is null)
        {
            throw new NotFoundException(nameof(Order), orderId);
        }

        if (order.Status != "Allocated")
        {
            throw new BadRequestException("Chỉ có thể tạo phiếu xuất cho đơn hàng đã phân bổ (Allocated).");
        }

        if (order.SubOrders.Count == 0)
        {
            throw new BadRequestException("Đơn hàng không có sub-order để tạo phiếu xuất.");
        }
        foreach (var subOrder in order.SubOrders)
        {
            var inventories = await _inventoryRepository.GetByWarehouseAsync(subOrder.WarehouseId);
            var inventoryMap = inventories.ToDictionary(i => i.ProductId);

            var stockDocumentLines = new List<CreateStockDocumentLineRequest>();

            foreach (var item in subOrder.SubOrderItems)
            {
                if (!inventoryMap.TryGetValue(item.ProductId, out var inventory))
                {
                    var allInventories = await _inventoryRepository.GetByTenantAsync(order.TenantId);
                    var productInventory = allInventories.FirstOrDefault(i => i.ProductId == item.ProductId && i.WarehouseId == subOrder.WarehouseId);
                    
                    if (productInventory == null)
                    {
                        throw new NotFoundException(nameof(WarehouseInventory), item.ProductId);
                    }

                    if (productInventory.WarehouseZoneId == Guid.Empty)
                    {
                        throw new BadRequestException($"Sản phẩm chưa được gán khu vực trong kho. Vui lòng gán khu vực cho sản phẩm trước khi tạo phiếu xuất.");
                    }

                    inventory = productInventory;
                }

                if (inventory.WarehouseZoneId == Guid.Empty)
                {
                    throw new BadRequestException($"Sản phẩm chưa được gán khu vực trong kho. Vui lòng gán khu vực cho sản phẩm trước khi tạo phiếu xuất.");
                }

                if (inventory.ReservedStock >= item.Quantity)
                {
                    inventory.ReservedStock -= item.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventoryRepository.UpdateAsync(inventory);
                }

                stockDocumentLines.Add(new CreateStockDocumentLineRequest(
                    item.ProductId,
                    inventory.WarehouseZoneId ?? Guid.Empty,
                    item.Quantity,
                    0
                ));
            }

            var stockDocumentRequest = new CreateStockDocumentRequest(
                order.TenantId,
                subOrder.WarehouseId,
                "Outbound",
                DateTime.UtcNow,
                order.CustomerName,
                order.OrderCode,
                $"Xuất hàng cho đơn {order.OrderCode}",
                stockDocumentLines
            );

            await _stockDocumentService.CreateAsync(stockDocumentRequest, createdBy);
        }

        await ReleaseStockReservationsAsync(order);
        order.Status = "Processing";
        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        return MapToDetail(order);
    }

    private async Task EnsureTenantExistsAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static OrderListItemDto MapToListItem(Order order)
    {
        var subOrderCount = order.SubOrders.Count;
        return new OrderListItemDto(
            order.Id,
            order.OrderCode,
            order.Channel,
            order.CustomerName,
            order.Status,
            order.TotalAmount,
            subOrderCount,
            subOrderCount > 1,
            order.CreatedAt);
    }

    private static OrderDetailDto MapToDetail(Order order)
    {
        var orderItems = order.OrderItems.Select(i => new OrderItemDetailDto(
            i.ProductId,
            i.Product?.Sku ?? string.Empty,
            i.Product?.Name ?? string.Empty,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice)).ToList();

        var subOrders = order.SubOrders.Select(s => new SubOrderDetailDto(
            s.Id,
            s.SubOrderCode,
            s.WarehouseId,
            s.Warehouse?.Name ?? string.Empty,
            s.Warehouse?.Code ?? string.Empty,
            s.Status,
            s.SubOrderItems.Select(i => new SubOrderItemDetailDto(
                i.ProductId,
                i.Product?.Sku ?? string.Empty,
                i.Product?.Name ?? string.Empty,
                i.Quantity)).ToList())).ToList();

        return new OrderDetailDto(
            order.Id,
            order.TenantId,
            order.OrderCode,
            order.Channel,
            order.CustomerName,
            order.CustomerPhone,
            order.CustomerAddress,
            order.CustomerLatitude,
            order.CustomerLongitude,
            order.TotalAmount,
            order.ShippingFee,
            order.Status,
            order.CreatedAt,
            orderItems,
            subOrders);
    }
}
