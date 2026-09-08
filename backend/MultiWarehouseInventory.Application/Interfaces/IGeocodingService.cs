namespace MultiWarehouseInventory.Application.Interfaces;

public interface IGeocodingService
{
    Task<(decimal Latitude, decimal Longitude)> ResolveCoordinatesAsync(
        string address,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default);
}
