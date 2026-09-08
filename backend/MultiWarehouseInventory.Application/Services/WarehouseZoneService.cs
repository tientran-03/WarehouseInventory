using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class WarehouseZoneService : IWarehouseZoneService
{
    private readonly IWarehouseZoneRepository _zoneRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseInventoryRepository _inventoryRepository;

    public WarehouseZoneService(
        IWarehouseZoneRepository zoneRepository,
        ITenantRepository tenantRepository,
        IWarehouseRepository warehouseRepository,
        IWarehouseInventoryRepository inventoryRepository)
    {
        _zoneRepository = zoneRepository;
        _tenantRepository = tenantRepository;
        _warehouseRepository = warehouseRepository;
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IEnumerable<ZoneResponse>> GetByTenantAsync(Guid tenantId)
    {
        await EnsureTenantAsync(tenantId);
        var zones = await _zoneRepository.GetByTenantAsync(tenantId);
        return zones.Select(Map);
    }

    public async Task<ZoneResponse> CreateAsync(CreateZoneRequest request)
    {
        await EnsureTenantAsync(request.TenantId);

        var warehouse = await _warehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse is null || !warehouse.IsActive || warehouse.TenantId != request.TenantId)
        {
            throw new NotFoundException(nameof(Warehouse), request.WarehouseId);
        }

        if (request.Capacity <= 0)
        {
            throw new BadRequestException("Sức chứa phải lớn hơn 0.");
        }

        var zoneCode = request.Code.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(zoneCode) || string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Mã và tên khu vực không được để trống.");
        }

        if (await _zoneRepository.AnyCodeExistsInWarehouseAsync(request.WarehouseId, zoneCode))
        {
            throw new BadRequestException($"Mã khu vực '{zoneCode}' đã tồn tại trong kho này.");
        }

        var zone = new WarehouseZone
        {
            TenantId = request.TenantId,
            WarehouseId = request.WarehouseId,
            Code = zoneCode,
            Name = request.Name.Trim(),
            Capacity = request.Capacity,
            UsedCapacity = 0,
            CreatedAt = DateTime.UtcNow,
        };

        await _zoneRepository.AddAsync(zone);
        await _zoneRepository.SaveChangesAsync();

        var saved = await _zoneRepository.GetByIdAsync(zone.Id)
            ?? throw new InvalidOperationException("Không thể tải khu vực vừa tạo.");
        return Map(saved);
    }

    public async Task DeleteAsync(Guid id)
    {
        var zone = await _zoneRepository.GetByIdAsync(id);
        if (zone is null)
        {
            throw new NotFoundException(nameof(WarehouseZone), id);
        }

        // Kiểm tra xem có inventory nào đang tham chiếu đến zone này không
        var hasInventory = await _inventoryRepository.HasInventoryInZoneAsync(id);
        if (hasInventory)
        {
            throw new BadRequestException("Không thể xóa khu vực đang có tồn kho. Hãy chuyển hoặc xóa tồn kho trước.");
        }

        try
        {
            await _zoneRepository.DeleteAsync(zone);
            await _zoneRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Xử lý lỗi database constraint
            var errorMessage = ex.InnerException?.Message ?? ex.Message;
            if (errorMessage.Contains("foreign key constraint") == true ||
                errorMessage.Contains("FOREIGN KEY") == true ||
                errorMessage.Contains("Cannot delete") == true)
            {
                throw new BadRequestException("Không thể xóa khu vực do có dữ liệu liên quan (lịch sử giao dịch, phiếu nhập/xuất). Hãy xóa hoặc chuyển dữ liệu liên quan trước.");
            }
            throw new BadRequestException($"Không thể xóa khu vực: {errorMessage}");
        }
    }

    private async Task EnsureTenantAsync(Guid tenantId)
    {
        var tenant = await _tenantRepository.GetByIdAsync(tenantId);
        if (tenant is null || !tenant.IsActive)
        {
            throw new NotFoundException(nameof(Tenant), tenantId);
        }
    }

    private static ZoneResponse Map(WarehouseZone z) => new(
        z.Id,
        z.TenantId,
        z.WarehouseId,
        z.Warehouse?.Name ?? string.Empty,
        z.Code,
        z.Name,
        z.Capacity,
        z.UsedCapacity);
}
