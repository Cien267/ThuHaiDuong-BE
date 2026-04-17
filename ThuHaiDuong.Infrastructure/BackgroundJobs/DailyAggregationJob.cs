using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThuHaiDuong.Domain.InterfaceRepositories;

namespace ThuHaiDuong.Infrastructure.BackgroundJobs;

public class DailyAggregationJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyAggregationJob> _logger;
 
    // Chạy lúc 00:05 UTC để tránh race condition với midnight boundary
    private static readonly TimeOnly TargetTime = new(0, 5, 0);
 
    public DailyAggregationJob(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyAggregationJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }
 
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DailyAggregationJob started.");
 
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = CalculateDelayUntilNextRun();
            _logger.LogInformation(
                "DailyAggregationJob: next run in {Delay:hh\\:mm\\:ss}", delay);
 
            await Task.Delay(delay, stoppingToken);
 
            if (stoppingToken.IsCancellationRequested) break;
 
            await RunAsync(stoppingToken);
        }
    }
 
    private async Task RunAsync(CancellationToken cancellationToken)
    {
        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        _logger.LogInformation(
            "DailyAggregationJob: aggregating stats for {Date}", yesterday);
 
        try
        {
            // Tạo scope mới vì IAnalyticsRepository là scoped service
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider
                .GetRequiredService<IAnalyticsRepository>();
 
            await repo.AggregateDailyStatsAsync(yesterday);
 
            _logger.LogInformation(
                "DailyAggregationJob: completed for {Date}", yesterday);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "DailyAggregationJob: failed for {Date}", yesterday);
            // Không rethrow — job sẽ tiếp tục chạy vào hôm sau
        }
    }
 
    private static TimeSpan CalculateDelayUntilNextRun()
    {
        var now     = DateTime.UtcNow;
        var today   = now.Date;
        var nextRun = today.Add(TargetTime.ToTimeSpan());
 
        // Nếu đã qua giờ chạy hôm nay → chạy ngày mai
        if (now >= nextRun)
            nextRun = nextRun.AddDays(1);
 
        return nextRun - now;
    }
}