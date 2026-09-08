using Microsoft.Extensions.Logging;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class OrderAllocationService : IOrderAllocationService
{
    private const int ReservationMinutes = 30;

    private readonly IGeocodingService _geocodingService;
    private readonly IWarehouseCacheService _warehouseCacheService;
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderNotificationService _orderNotificationService;
    private readonly IWmsNotificationService _wmsNotificationService;
    private readonly ILogger<OrderAllocationService> _logger;

    public OrderAllocationService(
        IGeocodingService geocodingService,
        IWarehouseCacheService warehouseCacheService,
        IWarehouseInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        ITenantRepository tenantRepository,
        IUnitOfWork unitOfWork,
        IOrderNotificationService orderNotificationService,
        IWmsNotificationService wmsNotificationService,
        ILogger<OrderAllocationService> logger)
    {
        _geocodingService = geocodingService;
        _warehouseCacheService = warehouseCacheService;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _tenantRepository = tenantRepository;
        _unitOfWork = unitOfWork;
        _orderNotificationService = orderNotificationService;
        _wmsNotificationService = wmsNotificationService;
        _logger = logger;
    }

    public async Task<OrderAllocationResultDto> ProcessIncomingOrderAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processing incoming order {OrderCode} from channel {Channel} for tenant {TenantId}",
            request.OrderCode,
            request.Channel,
            request.TenantId);

        var existing = await _orderRepository.GetByOrderCodeAsync(request.TenantId, request.OrderCode);
        if (existing is not null)
        {
            throw new BadRequestException($"Mã đơn '{request.OrderCode}' đã tồn tại trong hệ thống.");
        }

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), request.TenantId);
        }

        var (customerLat, customerLon) = await _geocodingService.ResolveCoordinatesAsync(
            request.CustomerAddress,
            request.CustomerLatitude,
            request.CustomerLongitude,
            cancellationToken);

        var warehouses = await _warehouseCacheService.GetActiveWarehousesAsync(request.TenantId, cancellationToken);
        var warehouseIds = warehouses.Select(w => w.Id).ToList();
        var productIds = request.OrderItems.Select(i => i.ProductId).Distinct().ToList();

        var inventories = await _inventoryRepository.GetByWarehousesAndProductsAsync(
            request.TenantId,
            warehouseIds,
            productIds);

        foreach (var item in request.OrderItems)
        {
            if (!inventories.Any(i => i.ProductId == item.ProductId))
            {
                throw new WarehouseInventoryNotFoundException(Guid.Empty, item.ProductId);
            }
        }

        var allocationLines = OrderAllocationPlanner.Plan(
            request.OrderItems,
            warehouses,
            inventories,
            customerLat,
            customerLon);

        var warehouseLookup = warehouses.ToDictionary(w => w.Id);
        var groupedByWarehouse = allocationLines
            .GroupBy(x => x.WarehouseId)
            .OrderBy(g => g.Min(x => x.DistanceKm))
            .ToList();

        var orderId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddMinutes(ReservationMinutes);
        var order = BuildOrder(request, orderId, customerLat, customerLon);
        var subOrderResults = new List<SubOrderAllocationDto>();
        var inventoriesToUpdate = new List<WarehouseInventory>();
        var inventoryMap = inventories
            .GroupBy(i => (i.WarehouseId, i.ProductId))
            .ToDictionary(
                group => group.Key,
                group => group.OrderBy(i => i.WarehouseZone?.Code).ToList());

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var subOrderIndex = 1;

            foreach (var warehouseGroup in groupedByWarehouse)
            {
                var warehouse = warehouseLookup[warehouseGroup.Key];
                var subOrderId = Guid.NewGuid();
                var subOrderCode = $"{request.OrderCode}-WH{subOrderIndex}";

                var subOrder = new SubOrder
                {
                    Id = subOrderId,
                    OrderId = orderId,
                    WarehouseId = warehouse.Id,
                    SubOrderCode = subOrderCode,
                    Status = "PendingPick",
                    CreatedAt = DateTime.UtcNow,
                };

                var subOrderItems = new List<SubOrderItemAllocationDto>();

                foreach (var line in warehouseGroup)
                {
                    if (!inventoryMap.TryGetValue((line.WarehouseId, line.ProductId), out var inventoriesForProduct))
                    {
                        throw new WarehouseInventoryNotFoundException(line.WarehouseId, line.ProductId);
                    }

                    var quantityToReserve = line.Quantity;
                    foreach (var inventory in inventoriesForProduct)
                    {
                        var reservable = Math.Min(quantityToReserve, inventory.AvailableStock);
                        if (reservable <= 0)
                        {
                            continue;
                        }

                        inventory.ReservedStock += reservable;
                        inventory.UpdatedAt = DateTime.UtcNow;
                        inventoriesToUpdate.Add(inventory);
                        quantityToReserve -= reservable;

                        if (quantityToReserve == 0)
                        {
                            break;
                        }
                    }

                    if (quantityToReserve > 0)
                    {
                        throw new InsufficientStockException(
                            line.WarehouseId,
                            line.ProductId,
                            line.Quantity,
                            line.Quantity - quantityToReserve);
                    }

                    subOrder.SubOrderItems.Add(new SubOrderItem
                    {
                        ProductId = line.ProductId,
                        Quantity = line.Quantity,
                        CreatedAt = DateTime.UtcNow,
                    });

                    order.StockReservations.Add(new StockReservation
                    {
                        TenantId = request.TenantId,
                        WarehouseId = line.WarehouseId,
                        ProductId = line.ProductId,
                        OrderId = orderId,
                        Quantity = line.Quantity,
                        Status = "Active",
                        ExpiresAt = expiresAt,
                        CreatedAt = DateTime.UtcNow,
                    });

                    subOrderItems.Add(new SubOrderItemAllocationDto(line.ProductId, line.Quantity));
                }

                order.SubOrders.Add(subOrder);

                subOrderResults.Add(new SubOrderAllocationDto(
                    subOrderId,
                    subOrderCode,
                    warehouse.Id,
                    warehouse.Name,
                    warehouseGroup.Min(x => x.DistanceKm),
                    subOrderItems));

                subOrderIndex++;
            }

            await _inventoryRepository.UpdateRangeAsync(inventoriesToUpdate.Distinct());
            await _orderRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }

        var result = new OrderAllocationResultDto(
            orderId,
            request.OrderCode,
            "Allocated",
            customerLat,
            customerLon,
            subOrderResults.Count,
            subOrderResults.Count > 1,
            subOrderResults);

        _logger.LogInformation(
            "Order {OrderCode} allocated to {SubOrderCount} warehouse(s). Split={IsSplit}",
            request.OrderCode,
            result.SubOrderCount,
            result.IsSplitOrder);

        await _orderNotificationService.NotifyWarehousesAsync(result, cancellationToken);
        await _wmsNotificationService.SendPickListRequestsAsync(result, cancellationToken);

        return result;
    }

    private static Order BuildOrder(
        CreateOrderRequest request,
        Guid orderId,
        decimal customerLat,
        decimal customerLon)
    {
        var order = new Order
        {
            Id = orderId,
            TenantId = request.TenantId,
            OrderCode = request.OrderCode,
            Channel = request.Channel,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            CustomerAddress = request.CustomerAddress,
            CustomerLatitude = customerLat,
            CustomerLongitude = customerLon,
            ShippingFee = request.ShippingFee,
            Status = "Allocated",
            CreatedAt = DateTime.UtcNow,
        };

        foreach (var item in request.OrderItems)
        {
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                CreatedAt = DateTime.UtcNow,
            };
            
            order.OrderItems.Add(orderItem);
        }

        order.TotalAmount = order.OrderItems.Sum(i => i.TotalPrice) + request.ShippingFee;
        return order;
    }
}
