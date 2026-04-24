using ThuHaiDuong.Application.ImplementService;
using ThuHaiDuong.Application.InterfaceService;
using ThuHaiDuong.Domain.Entities;
using ThuHaiDuong.Domain.InterfaceRepositories;
using ThuHaiDuong.Infrastructure.DataContext;
using ThuHaiDuong.Infrastructure.ImplementRepositories;

namespace ThuHaiDuong.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaseServices(this IServiceCollection services)
    {
        services.AddScoped<IDbContext, AppDbContext>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddHttpClient();
        services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
        return services;
    }
    
    public static IServiceCollection AddAuthServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        return services;
    }
    
    public static IServiceCollection AddEncryptionServices(this IServiceCollection services)
    {
        services.AddSingleton<IEncryptionService, EncryptionService>();
        return services;
    }
    
    public static IServiceCollection AddCategoryServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryService>(sp =>
            new CachedCategoryService(
                sp.GetRequiredService<CategoryService>(),
                sp.GetRequiredService<ICacheService>()));
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        return services;
    }
    
    public static IServiceCollection AddTagServices(this IServiceCollection services)
    {
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ITagRepository, TagRepository>();
        return services;
    }
    
    public static IServiceCollection AddAuthorServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IAuthorRepository, AuthorRepository>();
        return services;
    }
    
    public static IServiceCollection AddStoryServices(this IServiceCollection services)
    {
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<IStoryService>(sp =>
            new CachedStoryService(
                sp.GetRequiredService<StoryService>(),
                sp.GetRequiredService<ICacheService>()));
        services.AddScoped<IStoryRepository, StoryRepository>();
        return services;
    }
    
    public static IServiceCollection AddChapterServices(this IServiceCollection services)
    {
        services.AddScoped<IChapterService, ChapterService>();
        services.AddScoped<IChapterRepository, ChapterRepository>();
        return services;
    }
    
    public static IServiceCollection AddAffiliateServices(this IServiceCollection services)
    {
        services.AddScoped<IAffiliateService, AffiliateService>();
        services.AddScoped<IAffiliateRepository, AffiliateRepository>();
        return services;
    }
    
    public static IServiceCollection AddAnalyticsServices(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<IAnalyticsService>(sp =>
            new CachedAnalyticsService(
                sp.GetRequiredService<AnalyticsService>(),
                sp.GetRequiredService<ICacheService>()));
        services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
        return services;
    }
    
    public static IServiceCollection AddReadingProgressServices(this IServiceCollection services)
    {
        services.AddScoped<IReadingProgressService, ReadingProgressService>();
        services.AddScoped<IReadingProgressRepository, ReadingProgressRepository>();
        return services;
    }
    
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services
            .AddBaseServices()
            .AddAuthServices()
            .AddEncryptionServices()
            .AddCategoryServices()
            .AddTagServices()
            .AddAuthorServices()
            .AddStoryServices()
            .AddChapterServices()
            .AddAffiliateServices()
            .AddAnalyticsServices()
            .AddReadingProgressServices();

        return services;
    }

}