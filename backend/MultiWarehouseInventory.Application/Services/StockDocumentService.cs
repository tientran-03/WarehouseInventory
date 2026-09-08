using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Common;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class StockDocumentService : IStockDocumentService
{
    private readonly IStockDocumentRepository _documentRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IProductRepository _productRepository;
    private readonly IWarehouseZoneRepository _warehouseZoneRepository;
    private readonly IWarehouseInventoryRepository _inventoryRepository;
    private readonly IStockMovementRepository _movementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StockDocumentService(
        IStockDocumentRepository documentRepository,
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IProductRepository productRepository,
        IWarehouseZoneRepository warehouseZoneRepository,
        IWarehouseInventoryRepository inventoryRepository,
        IStockMovementRepository movementRepository,
        IUnitOfWork unitOfWork)
    {
        _documentRepository = documentRepository;
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _productRepository = productRepository;
        _warehouseZoneRepository = warehouseZoneRepository;
        _inventoryRepository = inventoryRepository;
        _movementRepository = movementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<StockDocumentResponse>> GetByTenantAsync(
        StockMovementReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var documents = await GetDocumentsAsync(filter, cancellationToken);
        return documents.Select(Map).ToList();
    }

    public async Task<StockDocumentResponse> CreateAsync(
        CreateStockDocumentRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        var type = NormalizeType(request.Type);
        await EnsureTenantAsync(request.TenantId);

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse is null || !warehouse.IsActive || warehouse.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        if (request.Lines is null || request.Lines.Count == 0)
        {
            throw new BadRequestException("Phiếu phải có ít nhất một dòng sản phẩm.");
        }

        if (request.Lines.Any(x => x.Quantity <= 0))
        {
            throw new BadRequestException("Số lượng trên mỗi dòng phải lớn hơn 0.");
        }

        if (request.Lines.Any(x => x.UnitPrice is < 0))
        {
            throw new BadRequestException("Đơn giá không được âm.");
        }

        if (request.Lines
            .GroupBy(x => new { x.ProductId, x.WarehouseZoneId })
            .Any(group => group.Count() > 1))
        {
            throw new BadRequestException("Mỗi sản phẩm chỉ được xuất hiện một lần trong cùng khu vực của phiếu.");
        }

        if (RequiresReason(type) && string.IsNullOrWhiteSpace(request.Note))
        {
            throw new BadRequestException("Phiếu tồn đầu/điều chỉnh phải có lý do hoặc ghi chú.");
        }

        var documentDate = request.DocumentDate?.ToUniversalTime() ?? DateTime.UtcNow;
        var document = new StockDocument
        {
            TenantId = request.TenantId,
            WarehouseId = request.WarehouseId,
            DocumentCode = GenerateDocumentCode(type),
            Type = type,
            Status = "Completed",
            DocumentDate = documentDate,
            PartnerName = TrimToNull(request.PartnerName),
            ReferenceCode = TrimToNull(request.ReferenceCode),
            Note = TrimToNull(request.Note),
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

        var zoneUsage = new Dictionary<Guid, int>();
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        foreach (var requestLine in request.Lines)
        {
            var product = await _productRepository.GetByIdAsync(requestLine.ProductId);
            if (product is null || !product.IsActive || product.TenantId != request.TenantId)
            {
                throw new NotFoundException(nameof(Product), requestLine.ProductId);
            }

            var zone = await _warehouseZoneRepository.GetByIdAsync(requestLine.WarehouseZoneId);
            if (zone is null || zone.TenantId != request.TenantId || zone.WarehouseId != request.WarehouseId)
            {
                throw new NotFoundException(nameof(WarehouseZone), requestLine.WarehouseZoneId);
            }

            if (!zoneUsage.TryGetValue(zone.Id, out var usedCapacity))
            {
                usedCapacity = await _inventoryRepository.GetOnHandStockByZoneAsync(zone.Id);
                zoneUsage[zone.Id] = usedCapacity;
            }

            var inventory = await _inventoryRepository.GetByWarehouseProductAndZoneAsync(
                request.WarehouseId,
                requestLine.ProductId,
                request.TenantId,
                zone.Id);
            var stockBefore = inventory?.OnHandStock ?? 0;

            if (IsStockIncrease(type))
            {
                if (usedCapacity + requestLine.Quantity > zone.Capacity)
                {
                    var remainingCapacity = Math.Max(0, zone.Capacity - usedCapacity);
                    throw new BadRequestException(
                        $"Khu vực {zone.Code} chỉ còn {remainingCapacity} sức chứa, không thể nhập {requestLine.Quantity}.");
                }

                if (type == StockDocumentTypes.OpeningBalance && inventory is not null)
                {
                    throw new BadRequestException(
                        $"Sản phẩm {product.Sku} đã có tồn tại khu vực {zone.Code}; hãy dùng Điều chỉnh tăng thay vì Tồn đầu kỳ.");
                }

                if (inventory is null)
                {
                    inventory = new WarehouseInventory
                    {
                        TenantId = request.TenantId,
                        WarehouseId = request.WarehouseId,
                        ProductId = requestLine.ProductId,
                        WarehouseZoneId = zone.Id,
                        LocationInWarehouse = zone.Code,
                        OnHandStock = requestLine.Quantity,
                        ReservedStock = 0,
                        ReorderLevel = 10,
                        CreatedAt = DateTime.UtcNow,
                    };
                    await _inventoryRepository.AddAsync(inventory);
                }
                else
                {
                    inventory.OnHandStock += requestLine.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;
                    await _inventoryRepository.UpdateAsync(inventory);
                }

                usedCapacity += requestLine.Quantity;
            }
            else
            {
                if (inventory is null)
                {
                    throw new WarehouseInventoryNotFoundException(request.WarehouseId, requestLine.ProductId);
                }

                if (inventory.AvailableStock < requestLine.Quantity)
                {
                    throw new InsufficientStockException(
                        request.WarehouseId,
                        requestLine.ProductId,
                        requestLine.Quantity,
                        inventory.AvailableStock);
                }

                inventory.OnHandStock -= requestLine.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
                usedCapacity -= requestLine.Quantity;
            }

            zoneUsage[zone.Id] = usedCapacity;
            zone.UsedCapacity = usedCapacity;
            zone.UpdatedAt = DateTime.UtcNow;
            await _warehouseZoneRepository.UpdateAsync(zone);

            var unitPrice = requestLine.UnitPrice ?? product.Price;
            document.Lines.Add(new StockDocumentLine
            {
                ProductId = product.Id,
                WarehouseZoneId = zone.Id,
                Quantity = requestLine.Quantity,
                UnitPrice = unitPrice,
                CreatedAt = DateTime.UtcNow,
            });

            await _movementRepository.AddAsync(new StockMovement
            {
                TenantId = request.TenantId,
                WarehouseId = request.WarehouseId,
                ProductId = product.Id,
                Type = type,
                QuantityChanged = IsStockIncrease(type) ? requestLine.Quantity : -requestLine.Quantity,
                StockBefore = stockBefore,
                StockAfter = inventory.OnHandStock,
                ReferenceCode = document.DocumentCode,
                Note = document.Note,
                CreatedBy = createdBy,
                StockDocument = document,
                CreatedAt = DateTime.UtcNow,
            });
        }

        await _documentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        var created = (await _documentRepository.GetByTenantAsync(
            request.TenantId,
            request.WarehouseId,
            null,
            null,
            null,
            cancellationToken))
            .SingleOrDefault(x => x.Id == document.Id)
            ?? throw new InvalidOperationException("Không thể tải lại phiếu nhập/xuất vừa tạo.");

        return Map(created);
    }

    public async Task<StockMovementReportResponse> GetReportAsync(
        StockMovementReportFilter filter,
        CancellationToken cancellationToken = default)
    {
        var documents = await GetDocumentsAsync(filter, cancellationToken);
        var rows = documents
            .SelectMany(document => document.Lines.Select(line => new StockMovementReportRow(
                document.DocumentDate,
                document.DocumentCode,
                document.Type,
                document.Warehouse.Name,
                line.Product.Sku,
                line.Product.Name,
                line.WarehouseZone.Code,
                line.Quantity,
                line.UnitPrice,
                line.Quantity * line.UnitPrice,
                document.PartnerName,
                document.ReferenceCode,
                document.Note)))
            .OrderByDescending(x => x.DocumentDate)
            .ThenBy(x => x.DocumentCode)
            .ToList();

        return new StockMovementReportResponse(
            filter.FromDate,
            filter.ToDate,
            documents.Count,
            rows.Where(x => x.Type == StockDocumentTypes.Inbound).Sum(x => x.Quantity),
            rows.Where(x => x.Type == StockDocumentTypes.Outbound).Sum(x => x.Quantity),
            rows.Where(x => x.Type == StockDocumentTypes.Inbound).Sum(x => x.LineTotal),
            rows.Where(x => x.Type == StockDocumentTypes.Outbound).Sum(x => x.LineTotal),
            rows.Where(x => x.Type == StockDocumentTypes.OpeningBalance).Sum(x => x.Quantity),
            rows.Where(x => x.Type == StockDocumentTypes.AdjustmentIn).Sum(x => x.Quantity),
            rows.Where(x => x.Type == StockDocumentTypes.AdjustmentOut).Sum(x => x.Quantity),
            rows.Where(x => x.Type == StockDocumentTypes.OpeningBalance).Sum(x => x.LineTotal),
            rows.Where(x => x.Type == StockDocumentTypes.AdjustmentIn).Sum(x => x.LineTotal),
            rows.Where(x => x.Type == StockDocumentTypes.AdjustmentOut).Sum(x => x.LineTotal),
            rows);
    }

    private async Task<List<StockDocument>> GetDocumentsAsync(
        StockMovementReportFilter filter,
        CancellationToken cancellationToken)
    {
        await EnsureTenantAsync(filter.TenantId);

        var type = string.IsNullOrWhiteSpace(filter.Type) ? null : NormalizeType(filter.Type);
        var fromDate = filter.FromDate?.Date;
        var toDate = filter.ToDate?.Date.AddDays(1).AddTicks(-1);
        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            throw new BadRequestException("Ngày bắt đầu không được sau ngày kết thúc.");
        }

        if (filter.WarehouseId.HasValue)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(filter.WarehouseId.Value);
            if (warehouse is null || warehouse.TenantId != filter.TenantId)
            {
                throw new NotFoundException(nameof(Warehouse), filter.WarehouseId.Value);
            }
        }

        return await _documentRepository.GetByTenantAsync(
            filter.TenantId,
            filter.WarehouseId,
            type,
            fromDate,
            toDate,
            cancellationToken);
    }

    private async Task EnsureTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static string NormalizeType(string? type)
    {
        if (string.Equals(type, StockDocumentTypes.Inbound, StringComparison.OrdinalIgnoreCase))
        {
            return StockDocumentTypes.Inbound;
        }

        if (string.Equals(type, StockDocumentTypes.Outbound, StringComparison.OrdinalIgnoreCase))
        {
            return StockDocumentTypes.Outbound;
        }

        if (string.Equals(type, StockDocumentTypes.OpeningBalance, StringComparison.OrdinalIgnoreCase))
        {
            return StockDocumentTypes.OpeningBalance;
        }

        if (string.Equals(type, StockDocumentTypes.AdjustmentIn, StringComparison.OrdinalIgnoreCase))
        {
            return StockDocumentTypes.AdjustmentIn;
        }

        if (string.Equals(type, StockDocumentTypes.AdjustmentOut, StringComparison.OrdinalIgnoreCase))
        {
            return StockDocumentTypes.AdjustmentOut;
        }

        throw new BadRequestException("Loại phiếu không hợp lệ.");
    }

    private static string GenerateDocumentCode(string type)
    {
        var prefix = type switch
        {
            StockDocumentTypes.Inbound => "PN",
            StockDocumentTypes.Outbound => "PX",
            StockDocumentTypes.OpeningBalance => "TD",
            StockDocumentTypes.AdjustmentIn => "DCT",
            StockDocumentTypes.AdjustmentOut => "DCG",
            _ => "PK",
        };
        return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"[..26].ToUpperInvariant();
    }

    private static bool IsStockIncrease(string type) => type is
        StockDocumentTypes.Inbound or
        StockDocumentTypes.OpeningBalance or
        StockDocumentTypes.AdjustmentIn;

    private static bool RequiresReason(string type) => type is
        StockDocumentTypes.OpeningBalance or
        StockDocumentTypes.AdjustmentIn or
        StockDocumentTypes.AdjustmentOut;

    private static string? TrimToNull(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static StockDocumentResponse Map(StockDocument document)
    {
        var lines = document.Lines
            .OrderBy(x => x.Product.Sku)
            .Select(line => new StockDocumentLineResponse(
                line.Id,
                line.ProductId,
                line.Product.Sku,
                line.Product.Name,
                line.WarehouseZoneId,
                line.WarehouseZone.Code,
                line.WarehouseZone.Name,
                line.Quantity,
                line.UnitPrice,
                line.Quantity * line.UnitPrice))
            .ToList();

        return new StockDocumentResponse(
            document.Id,
            document.TenantId,
            document.DocumentCode,
            document.Type,
            document.Status,
            document.WarehouseId,
            document.Warehouse.Name,
            document.DocumentDate,
            document.PartnerName,
            document.ReferenceCode,
            document.Note,
            document.CreatedBy,
            document.CreatedAt,
            lines,
            lines.Sum(x => x.Quantity),
            lines.Sum(x => x.LineTotal));
    }
}
