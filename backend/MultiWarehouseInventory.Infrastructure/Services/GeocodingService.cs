using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Exceptions;

namespace MultiWarehouseInventory.Infrastructure.Services;

public class GeocodingService : IGeocodingService
{
    private static readonly (string Keyword, decimal Lat, decimal Lon)[] CityHints =
    [
        ("hà nội", 21.0285m, 105.8542m),
        ("ha noi", 21.0285m, 105.8542m),
        ("hanoi", 21.0285m, 105.8542m),
        ("tp.hcm", 10.8231m, 106.6297m),
        ("hồ chí minh", 10.8231m, 106.6297m),
        ("ho chi minh", 10.8231m, 106.6297m),
        ("đà nẵng", 16.0544m, 108.2022m),
        ("da nang", 16.0544m, 108.2022m),
        ("hải phòng", 20.8449m, 106.6881m),
        ("hai phong", 20.8449m, 106.6881m),
        ("cần thơ", 10.0452m, 105.7469m),
        ("can tho", 10.0452m, 105.7469m),
    ];

    public Task<(decimal Latitude, decimal Longitude)> ResolveCoordinatesAsync(
        string address,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken = default)
    {
        if (latitude != 0 || longitude != 0)
        {
            return Task.FromResult((latitude, longitude));
        }

        var normalized = address.Trim().ToLowerInvariant();
        foreach (var hint in CityHints)
        {
            if (normalized.Contains(hint.Keyword, StringComparison.Ordinal))
            {
                return Task.FromResult((hint.Lat, hint.Lon));
            }
        }

        throw new BadRequestException(
            "");
    }
}
