using MultiWarehouseInventory.Domain.Entities;

namespace MultiWarehouseInventory.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
