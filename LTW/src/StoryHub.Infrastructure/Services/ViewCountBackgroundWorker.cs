using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Options;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class ViewCountBackgroundWorker : BackgroundService
{
    private readonly IViewCountBufferService _bufferService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ViewCountSettings _settings;
    private readonly ILogger<ViewCountBackgroundWorker> _logger;

    public ViewCountBackgroundWorker(
        IViewCountBufferService bufferService,
        IServiceScopeFactory scopeFactory,
        IOptions<ViewCountSettings> settings,
        ILogger<ViewCountBackgroundWorker> logger)
    {
        _bufferService = bufferService;
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ViewCountBackgroundWorker đã khởi động.");

        var storyViews = new Dictionary<int, long>();
        var chapterViews = new Dictionary<int, long>();
        var lastFlushTime = DateTimeOffset.UtcNow;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await foreach (var item in _bufferService.ReadAllAsync(stoppingToken))
                {
                    storyViews[item.StoryId] = storyViews.GetValueOrDefault(item.StoryId) + 1;
                    if (item.ChapterId.HasValue)
                    {
                        chapterViews[item.ChapterId.Value] = chapterViews.GetValueOrDefault(item.ChapterId.Value) + 1;
                    }

                    var shouldFlush = (DateTimeOffset.UtcNow - lastFlushTime).TotalSeconds >= _settings.FlushIntervalSeconds
                        || storyViews.Count >= _settings.BatchSize;

                    if (shouldFlush)
                    {
                        await FlushViewsAsync(storyViews, chapterViews, stoppingToken);
                        storyViews.Clear();
                        chapterViews.Clear();
                        lastFlushTime = DateTimeOffset.UtcNow;
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong vòng lặp đệm lượt xem.");
                await Task.Delay(2000, stoppingToken);
            }
        }

        // Final flush before shutdown
        if (storyViews.Count > 0 || chapterViews.Count > 0)
        {
            await FlushViewsAsync(storyViews, chapterViews, CancellationToken.None);
        }

        _logger.LogInformation("ViewCountBackgroundWorker đã dừng an toàn.");
    }

    private async Task FlushViewsAsync(
        Dictionary<int, long> storyViews,
        Dictionary<int, long> chapterViews,
        CancellationToken cancellationToken)
    {
        if (storyViews.Count == 0 && chapterViews.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<StoryHubDbContext>();

        try
        {
            // Atomic database increments without entity tracking
            foreach (var (storyId, count) in storyViews)
            {
                await context.Stories
                    .Where(s => s.Id == storyId)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.ViewCount, x => x.ViewCount + count), cancellationToken);
            }

            foreach (var (chapterId, count) in chapterViews)
            {
                await context.Chapters
                    .Where(c => c.Id == chapterId)
                    .ExecuteUpdateAsync(c => c.SetProperty(x => x.ViewCount, x => x.ViewCount + count), cancellationToken);
            }

            _logger.LogDebug("Đã cập nhật lượt xem: {StoryCount} truyện, {ChapterCount} chương.", storyViews.Count, chapterViews.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi ghi dữ liệu lượt xem vào cơ sở dữ liệu.");
        }
    }
}
