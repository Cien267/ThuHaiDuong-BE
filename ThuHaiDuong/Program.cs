using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Infrastructure.DataContext;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Middlewares;
using ThuHaiDuong.Extensions;
using ThuHaiDuong.Filters;
using ThuHaiDuong.Infrastructure.BackgroundJobs;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAllServices(builder.Configuration);
builder.Services.AddHostedService<DailyAggregationJob>();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => e.ErrorMessage)
                    .ToArray()
            );
 
        var response = new
        {
            message    = "Validation failed.",
            statusCode = 400,
            errors,
        };
 
        return new BadRequestObjectResult(response);
    };
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        throw new ResponseErrorObject($"An error occurred while seeding the database: {ex.Message}", StatusCodes.Status400BadRequest);
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var allowedIPs = builder.Configuration
    .GetSection("Hangfire:AllowedIPs")
    .Get<string[]>();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new HangfireAuthorizationFilter(
            requiredRoles: new[] { "Administrator" },
            allowedIPs: allowedIPs ?? Array.Empty<string>()
        )
    }
});

app.MapControllers();

app.Run();
