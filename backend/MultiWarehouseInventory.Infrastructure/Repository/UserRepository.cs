using Microsoft.EntityFrameworkCore;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure.Data;

namespace MultiWarehouseInventory.Infrastructure.Repository;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public Task<User?> GetByEmailVerificationTokenHashAsync(string tokenHash)
    {
        return _context.Users.FirstOrDefaultAsync(u => u.EmailVerificationTokenHash == tokenHash);
    }

    public Task<bool> AnyUsernameExistsAsync(string username)
    {
        return _context.Users.AnyAsync(u => u.Username == username);
    }

    public Task<bool> AnyEmailExistsAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<IReadOnlyList<User>> GetByTenantIdAsync(Guid tenantId)
    {
        return await _context.Users.Where(u => u.TenantId == tenantId && u.IsActive).ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetAllCompanyAccountsAsync()
    {
        return await _context.Users.Where(u => u.TenantId != null && u.IsActive).ToListAsync();
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
