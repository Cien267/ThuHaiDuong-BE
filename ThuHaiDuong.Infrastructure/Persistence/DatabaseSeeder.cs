using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Infrastructure.DataContext;

namespace ThuHaiDuong.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope   = serviceProvider.CreateScope();
        var context       = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config        = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger        = scope.ServiceProvider
                                 .GetRequiredService<ILogger<AppDbContext>>();
 
        try
        {
            // Đảm bảo DB đã được migrate trước khi seed
            await context.Database.MigrateAsync();
 
            await SeedSuperAdminAsync(context, config, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding database.");
            throw;
        }
    }
 
    private static async Task SeedSuperAdminAsync(
        AppDbContext    context,
        IConfiguration  config,
        ILogger         logger)
    {
        // Đọc config từ appsettings.json hoặc environment variable
        var email    = config["SuperAdmin:Email"];
        var password = config["SuperAdmin:Password"];
        var userName = config["SuperAdmin:UserName"] ?? "superadmin";
        var fullName = config["SuperAdmin:FullName"] ?? "Super Administrator";
 
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning(
                "SuperAdmin seed skipped: SuperAdmin:Email or SuperAdmin:Password " +
                "not configured in appsettings.");
            return;
        }
 
        // Kiểm tra đã có SuperAdmin chưa — tránh tạo duplicate
        var exists = await context.Users
            .IgnoreQueryFilters()           // bypass Global Query Filter (DeletedAt)
            .AnyAsync(u => u.Role == "SuperAdmin");
 
        if (exists)
        {
            logger.LogInformation("SuperAdmin already exists. Seed skipped.");
            return;
        }
 
        var superAdmin = new User
        {
            Id           = Guid.NewGuid(),
            UserName     = userName,
            Email        = email.ToLower().Trim(),
            Password     = BCrypt.Net.BCrypt.HashPassword(password),
            FullName     = fullName,
            Role         = "SuperAdmin",
            IsActive     = true,
            CreatedAt    = DateTime.UtcNow,
            UpdatedAt    = DateTime.UtcNow,
        };
 
        await context.Users.AddAsync(superAdmin);
        await context.SaveChangesAsync();
 
        logger.LogInformation(
            "SuperAdmin seeded successfully. Email: {Email}", email);
    }
}