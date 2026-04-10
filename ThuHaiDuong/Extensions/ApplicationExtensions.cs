namespace ThuHaiDuong.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure
        services.AddInfrastructure(configuration);

        // Auth
        services.AddAuthServices(configuration);

        // Web (Controllers, Swagger, CORS)
        services.AddWebServices(configuration);

        // Application services
        services.AddApplicationServices();

        return services;
    }
}