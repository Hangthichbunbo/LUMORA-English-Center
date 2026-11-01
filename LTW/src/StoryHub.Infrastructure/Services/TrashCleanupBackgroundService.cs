using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StoryHub.Application.Interfaces;

namespace StoryHub.Infrastructure.Services;

public class TrashCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TrashCleanupBackgroundService> _logger;

    public TrashCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<TrashCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TrashCleanupBackgroundService (Tự động xóa vĩnh viễn thùng rác sau 7 ngày) đã khởi chạy.");

        // Wait 1 minute after application startup before initial cleanup run
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var trashService = scope.ServiceProvider.GetRequiredService<ITrashService>();
                var cleanedCount = await trashService.CleanupExpiredItemsAsync(stoppingToken);
                if (cleanedCount > 0)
                {
                    _logger.LogInformation("TrashCleanupBackgroundService: Đã tự động dọn dẹp vĩnh viễn {Count} mục hết hạn 7 ngày.", cleanedCount);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong quá trình dọn dẹp thùng rác tự động.");
            }

            try
            {
                // Run hourly check
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}

