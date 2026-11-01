using StoryHub.Domain.Common;
using StoryHub.Domain.Enums;

namespace StoryHub.Domain.Entities;

public class Story : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? AlternativeTitle { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }

    public int AuthorId { get; set; }
    public virtual Author Author { get; set; } = null!;

    public StoryStatus Status { get; set; } = StoryStatus.Draft;
    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public bool IsFeatured { get; set; }

    public long ViewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }

    // Admin who created the story (Only Admin can create stories)
    public string CreatedBy { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public string? RemovalReason { get; set; }

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];

    // Navigations
    public virtual ICollection<StoryCategory> StoryCategories { get; set; } = new List<StoryCategory>();
    public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<FollowedStory> FollowedStories { get; set; } = new List<FollowedStory>();
    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
}
