namespace StoryHub.Application.Common.Interfaces;

public record ViewCountItem(int StoryId, int? ChapterId, DateTimeOffset Timestamp);

public interface IViewCountBufferService
{
    ValueTask EnqueueViewAsync(int storyId, int? chapterId = null);
    IAsyncEnumerable<ViewCountItem> ReadAllAsync(CancellationToken cancellationToken);
}
