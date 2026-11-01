using Microsoft.AspNetCore.Identity;
using StoryHub.Domain.Common;

namespace StoryHub.Domain.Entities;

public class ApplicationUser : IdentityUser, IAuditableEntity
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? CommentBlockedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation collections
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public virtual ICollection<CommentReport> Reports { get; set; } = new List<CommentReport>();
    public virtual ICollection<FollowedStory> FollowedStories { get; set; } = new List<FollowedStory>();
    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();
}
