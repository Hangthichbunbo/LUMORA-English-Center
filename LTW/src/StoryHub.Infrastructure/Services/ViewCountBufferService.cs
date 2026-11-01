using System.Threading.Channels;
using StoryHub.Application.Common.Interfaces;

namespace StoryHub.Infrastructure.Services;

public class ViewCountBufferService : IViewCountBufferService
{
    private readonly Channel<ViewCountItem> _channel;

    public ViewCountBufferService()
    {
        // Bounded channel to prevent unbounded memory growth during high loads
        var options = new BoundedChannelOptions(50000)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        };
        _channel = Channel.CreateBounded<ViewCountItem>(options);
    }

    public async ValueTask EnqueueViewAsync(int storyId, int? chapterId = null)
    {
        await _channel.Writer.WriteAsync(new ViewCountItem(storyId, chapterId, DateTimeOffset.UtcNow));
    }

    public IAsyncEnumerable<ViewCountItem> ReadAllAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
