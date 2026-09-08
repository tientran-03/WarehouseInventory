using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MultiWarehouseInventory.Domain.Entities;
using MultiWarehouseInventory.Domain.Enums;
using MultiWarehouseInventory.Domain.Interfaces;
using MultiWarehouseInventory.Infrastructure;

namespace MultiWarehouseInventory.Infrastructure.Data;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>();
        var accountSecurityService = scope.ServiceProvider.GetRequiredService<IAccountSecurityService>();

        var username = configuration["AdminUser:Username"];
        var password = configuration["AdminUser:Password"];
        var email = configuration["AdminUser:Email"] ?? "admin@warehouse.com";
        var roleText = configuration["AdminUser:Role"] ?? "Admin";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AdminUser chưa được cấu hình trong appsettings — bỏ qua seed tài khoản admin.");
            return;
        }

        if (!Enum.TryParse<UserRole>(roleText, true, out var role))
        {
            role = UserRole.Admin;
        }

        await context.Database.MigrateAsync();

        var existing = await context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (existing is not null)
        {
            logger.LogInformation("Tài khoản admin '{Username}' đã tồn tại.", username);
            return;
        }

        context.Users.Add(new User
        {
            Username = username,
            Email = email,
            PasswordHash = accountSecurityService.HashPassword(password),
            Role = role,
            IsActive = true,
        });

        await context.SaveChangesAsync();
        logger.LogInformation("Đã tạo tài khoản admin '{Username}' với role {Role}.", username, role);
    }
}
