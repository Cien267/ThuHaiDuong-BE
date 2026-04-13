using ThuHaiDuong.Application.ImplementService;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Infrastructure.ImplementRepositories;

namespace ThuHaiDuong.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IDbContext, AppDbContext>();
        services.AddScoped<IBaseRepository<RefreshToken>, BaseRepository<RefreshToken>>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBaseRepository<User>, BaseRepository<User>>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        return services;
    }
    
    public static IServiceCollection AddEncryptionServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, EncryptionService>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddAuthServices()
            .AddEncryptionServices();

        return services;
    }

}