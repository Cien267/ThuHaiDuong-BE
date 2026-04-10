using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Shared.Constants;

namespace ThuHaiDuong.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(Constant.AppSettingKeys.DEFAULT_CONNECTION)
                               ?? throw new InvalidOperationException("Connection string is not configured");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString,
                b => b.MigrationsAssembly("ThuHaiDuong")));

        services.AddDataProtection();
        services.AddHttpContextAccessor();

        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout      = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout  = TimeSpan.FromMinutes(5),
                QueuePollInterval           = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks          = true
            }));

        services.AddHangfireServer();

        return services;
    }
}