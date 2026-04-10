using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Converter;

namespace ThuHaiDuong.Extensions;

public static class WebExtensions
{
    public static IServiceCollection AddWebServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCors(options =>
        {
            var allowedOrigins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            options.AddPolicy("AllowAll", policy =>
            {
                if (allowedOrigins?.Length > 0)
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                else
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
            });
        });

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new EmptyStringToNullConverter());
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(e => e.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors
                                       .Select(e => e.ErrorMessage)
                                       .ToArray()
                        );

                    throw new ResponseErrorObject(
                        "Validation failed",
                        StatusCodes.Status400BadRequest,
                        errors);
                };
            });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Auth API", Version = "v1" });

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In          = ParameterLocation.Header,
                Description = "Enter the token",
                Name        = "Authorization",
                Type        = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme      = "Bearer"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id   = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}