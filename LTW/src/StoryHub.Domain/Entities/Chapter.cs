using StoryHub.Domain.Common;
using StoryHub.Domain.Enums;

namespace StoryHub.Domain.Entities;

public class Chapter : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal ChapterNumber { get; set; }
    public string Content { get; set; } = string.Empty;

    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public long ViewCount { get; set; }

    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Navigations
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
}
