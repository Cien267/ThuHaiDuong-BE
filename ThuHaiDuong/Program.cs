using Microsoft.EntityFrameworkCore;
using ThuHaiDuong.Infrastructure.DataContext;
using Hangfire;
using ThuHaiDuong.Application.Payloads.Responses;
using ThuHaiDuong.Middlewares;
using ThuHaiDuong.Extensions;
using ThuHaiDuong.Filters;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAllServices(builder.Configuration);

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
