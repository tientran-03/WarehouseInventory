using AutoMapper;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Exceptions;
using MultiWarehouseInventory.Domain.Interfaces;

namespace MultiWarehouseInventory.Application.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly IWarehouseCacheService _warehouseCacheService;
    private readonly IMapper _mapper;

    public WarehouseService(
        IWarehouseRepository warehouseRepository,
        IWarehouseCacheService warehouseCacheService,
        IMapper mapper)
    {
        _warehouseRepository = warehouseRepository;
        _warehouseCacheService = warehouseCacheService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WarehouseResponse>> GetAllAsync()
    {
        var warehouses = await _warehouseRepository.GetAllActiveAsync();
        return _mapper.Map<IEnumerable<WarehouseResponse>>(warehouses);
    }

    public async Task<WarehouseResponse> GetByIdAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse is null || !warehouse.IsActive)
        {
            throw new NotFoundException(nameof(Warehouse), id);
        }

        return _mapper.Map<WarehouseResponse>(warehouse);
    }

    public async Task<WarehouseResponse> CreateAsync(UpsertWarehouseRequest request)
    {
        if (await _warehouseRepository.AnyCodeExistsAsync(request.Code))
        {
            throw new BadRequestException($"Mã kho '{request.Code}' đã tồn tại trong hệ thống.");
        }

        var warehouse = _mapper.Map<Warehouse>(request);
        warehouse.IsActive = true;

        await _warehouseRepository.AddAsync(warehouse);
        await _warehouseRepository.SaveChangesAsync();
        await _warehouseCacheService.InvalidateTenantAsync(request.TenantId);

        return _mapper.Map<WarehouseResponse>(warehouse);
    }

    public async Task UpdateAsync(Guid id, UpsertWarehouseRequest request)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse is null || !warehouse.IsActive)
        {
            throw new NotFoundException(nameof(Warehouse), id);
        }

        if (await _warehouseRepository.AnyCodeExistsForOtherAsync(request.Code, id))
        {
            throw new BadRequestException($"Mã kho '{request.Code}' đã được sử dụng bởi kho khác.");
        }

        _mapper.Map(request, warehouse);
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _warehouseRepository.UpdateAsync(warehouse);
        await _warehouseRepository.SaveChangesAsync();
        await _warehouseCacheService.InvalidateTenantAsync(warehouse.TenantId);
    }

    public async Task DeleteAsync(Guid id)
    {
        var warehouse = await _warehouseRepository.GetByIdAsync(id);
        if (warehouse is null)
        {
            throw new NotFoundException(nameof(Warehouse), id);
        }

        warehouse.IsActive = false;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _warehouseRepository.UpdateAsync(warehouse);
        await _warehouseRepository.SaveChangesAsync();
        await _warehouseCacheService.InvalidateTenantAsync(warehouse.TenantId);
    }
}
