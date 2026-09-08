

namespace MultiWarehouseInventory.Application.DTOs
{
    public record UpsertWarehouseRequest(
           Guid TenantId,
           string Code,
           string Name,
           string Address,
           string City,
           decimal Latitude,
           decimal Longitude
       );
    public record WarehouseResponse(
       Guid Id,
       Guid TenantId,
       string Code,
       string Name,
       string Address,
       string City,
       decimal Latitude,
       decimal Longitude,
       bool IsActive
       );

}
