using StoryHub.Domain.Common;

namespace StoryHub.Domain.Entities;

public class ReadingHistory : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public int ChapterId { get; set; }
    public virtual Chapter Chapter { get; set; } = null!;

    public DateTimeOffset LastReadAt { get; set; } = DateTimeOffset.UtcNow;
    public decimal Progress { get; set; } // 0 - 100 (%)
}
