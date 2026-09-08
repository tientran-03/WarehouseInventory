using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class TransferService : ITransferService
{
    private readonly IWarehouseTransferRepository _transferRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly IWarehouseZoneRepository _warehouseZoneRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferService(
        IWarehouseTransferRepository transferRepository,
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseInventoryRepository inventoryRepository,
        IWarehouseZoneRepository warehouseZoneRepository,
        IStockMovementRepository movementRepository,
        IUnitOfWork unitOfWork)
    {
        _transferRepository = transferRepository;
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _warehouseZoneRepository = warehouseZoneRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<TransferResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantAsync(tenantId);
        var items = await _transferRepository.GetByTenantAsync(tenantId);
        return items.Select(Map);
    }

    public async Task<TransferResponse> CreateAsync(CreateTransferRequest request)
    {
        await EnsureTenantAsync(request.TenantId);
        await ValidateWarehousesAsync(request);
        await ValidateProductAsync(request.TenantId, request.ProductId);

        if (request.Quantity <= 0)
        {
            throw new BadRequestException("Số lượng phải lớn hơn 0.");
        }

        if (request.FromWarehouseId == request.ToWarehouseId)
        {
            throw new BadRequestException("Kho xuất và kho nhận phải khác nhau.");
        }

        var transfer = new WarehouseTransfer
        {
            TenantId = request.TenantId,
            TransferCode = $"TRF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            FromWarehouseId = request.FromWarehouseId,
            ToWarehouseId = request.ToWarehouseId,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
        };

        await _transferRepository.AddAsync(transfer);
        await _transferRepository.SaveChangesAsync();

        var saved = await _transferRepository.GetByIdAsync(transfer.Id)
            ?? throw new InvalidOperationException("Không thể tải phiếu điều chuyển vừa tạo.");
        return Map(saved);
    }

    public async Task<TransferResponse> StartAsync(Guid id)
    {
        var transfer = await GetTransferOrThrow(id);
        if (transfer.Status != "Pending")
        {
            throw new BadRequestException("Chỉ phiếu Chờ xử lý mới có thể chuyển sang Đang vận chuyển.");
        }

        transfer.Status = "InTransit";
        transfer.UpdatedAt = DateTime.UtcNow;
        await _transferRepository.UpdateAsync(transfer);
        await _transferRepository.SaveChangesAsync();

        return Map(await _transferRepository.GetByIdAsync(id) ?? transfer);
    }

    public async Task<TransferResponse> CompleteAsync(Guid id)
    {
        var transfer = await GetTransferOrThrow(id);
        if (transfer.Status == "Completed")
        {
            throw new BadRequestException("Phiếu đã hoàn thành.");
        }

        await using var tx = await _unitOfWork.BeginTransactionAsync();

        var fromInventories = await _inventoryRepository.GetByWarehouseAndProductAsync(
            transfer.FromWarehouseId, transfer.ProductId, transfer.TenantId);

        var availableStock = fromInventories.Sum(inventory => inventory.AvailableStock);
        if (availableStock < transfer.Quantity)
        {
            throw new BadRequestException("Kho xuất không đủ tồn khả dụng.");
        }

        var fromBefore = fromInventories.Sum(inventory => inventory.OnHandStock);
        var quantityRemaining = transfer.Quantity;
        var zoneUsageReductions = new Dictionary<Guid, int>();

        foreach (var fromInventory in fromInventories.Where(inventory => inventory.AvailableStock > 0))
        {
            var quantityToMove = Math.Min(quantityRemaining, fromInventory.AvailableStock);
            if (quantityToMove == 0)
            {
                continue;
            }

            fromInventory.OnHandStock -= quantityToMove;
            fromInventory.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(fromInventory);

            if (fromInventory.WarehouseZoneId.HasValue)
            {
                zoneUsageReductions.TryGetValue(fromInventory.WarehouseZoneId.Value, out var reducedQuantity);
                zoneUsageReductions[fromInventory.WarehouseZoneId.Value] = reducedQuantity + quantityToMove;
            }

            quantityRemaining -= quantityToMove;
            if (quantityRemaining == 0)
            {
                break;
            }
        }

        var toInv = await _inventoryRepository.GetByWarehouseProductAndZoneAsync(
            transfer.ToWarehouseId, transfer.ProductId, transfer.TenantId, null);

        if (toInv is null)
        {
            toInv = new WarehouseInventory
            {
                TenantId = transfer.TenantId,
                WarehouseId = transfer.ToWarehouseId,
                ProductId = transfer.ProductId,
                OnHandStock = transfer.Quantity,
                ReservedStock = 0,
                ReorderLevel = 10,
                CreatedAt = DateTime.UtcNow,
            };
            await _inventoryRepository.AddAsync(toInv);

            await _movementRepository.AddAsync(new StockMovement
            {
                TenantId = transfer.TenantId,
                WarehouseId = transfer.ToWarehouseId,
                ProductId = transfer.ProductId,
                Type = "TransferIn",
                QuantityChanged = transfer.Quantity,
                StockBefore = 0,
                StockAfter = transfer.Quantity,
                ReferenceCode = transfer.TransferCode,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.UtcNow,
            });
        }
        else
        {
            var toBefore = toInv.OnHandStock;
            toInv.OnHandStock += transfer.Quantity;
            toInv.UpdatedAt = DateTime.UtcNow;
            await _inventoryRepository.UpdateAsync(toInv);

            await _movementRepository.AddAsync(new StockMovement
            {
                TenantId = transfer.TenantId,
                WarehouseId = transfer.ToWarehouseId,
                ProductId = transfer.ProductId,
                Type = "TransferIn",
                QuantityChanged = transfer.Quantity,
                StockBefore = toBefore,
                StockAfter = toInv.OnHandStock,
                ReferenceCode = transfer.TransferCode,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.UtcNow,
            });
        }

        foreach (var (zoneId, reducedQuantity) in zoneUsageReductions)
        {
            var zone = await _warehouseZoneRepository.GetByIdAsync(zoneId);
            if (zone is null)
            {
                continue;
            }

            zone.UsedCapacity = Math.Max(0, zone.UsedCapacity - reducedQuantity);
            zone.UpdatedAt = DateTime.UtcNow;
            await _warehouseZoneRepository.UpdateAsync(zone);
        }

        await _movementRepository.AddAsync(new StockMovement
        {
            TenantId = transfer.TenantId,
            WarehouseId = transfer.FromWarehouseId,
            ProductId = transfer.ProductId,
            Type = "TransferOut",
            QuantityChanged = -transfer.Quantity,
            StockBefore = fromBefore,
            StockAfter = fromBefore - transfer.Quantity,
            ReferenceCode = transfer.TransferCode,
            CreatedBy = Guid.Empty,
            CreatedAt = DateTime.UtcNow,
        });

        transfer.Status = "Completed";
        transfer.UpdatedAt = DateTime.UtcNow;
        await _transferRepository.UpdateAsync(transfer);

        await _inventoryRepository.SaveChangesAsync();
        await _movementRepository.SaveChangesAsync();
        await _transferRepository.SaveChangesAsync();
        await tx.CommitAsync();

        return Map(await _transferRepository.GetByIdAsync(id) ?? transfer);
    }

    private async Task<WarehouseTransfer> GetTransferOrThrow(Guid id)
    {
        var transfer = await _transferRepository.GetByIdAsync(id);
        if (transfer is null)
        {
            throw new NotFoundException(nameof(WarehouseTransfer), id);
        }

        return transfer;
    }

    private async Task EnsureTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private async Task ValidateWarehousesAsync(CreateTransferRequest request)
    {
        var from = await _warehouseRepository.GetByIdAsync(request.FromWarehouseId);
        var to = await _warehouseRepository.GetByIdAsync(request.ToWarehouseId);

        if (from is null || !from.IsActive || from.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.FromWarehouseId);
        }

        if (to is null || !to.IsActive || to.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.ToWarehouseId);
        }
    }

    private async Task ValidateProductAsync(Guid tenantId, Guid productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null || !product.IsActive || product.TenantId != tenantId)
        {
            throw new NotFoundException(nameof(Product), productId);
        }
    }

    private static TransferResponse Map(WarehouseTransfer t) => new(
        t.Id,
        t.TenantId,
        t.TransferCode,
        t.ProductId,
        t.Product?.Name ?? string.Empty,
        t.Product?.Sku ?? string.Empty,
        t.Quantity,
        t.FromWarehouseId,
        t.FromWarehouse?.Name ?? string.Empty,
        t.ToWarehouseId,
        t.ToWarehouse?.Name ?? string.Empty,
        t.Status,
        t.CreatedAt);
}
