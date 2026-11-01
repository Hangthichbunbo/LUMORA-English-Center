using StoryHub.Domain.Common;
using StoryHub.Domain.Enums;

namespace StoryHub.Domain.Entities;

public class CommentReport : BaseEntity
{
    public int CommentId { get; set; }
    public virtual Comment Comment { get; set; } = null!;

    public string ReportedByUserId { get; set; } = string.Empty;
    public virtual ApplicationUser ReportedByUser { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public string? ReviewedByUserId { get; set; }
    public virtual ApplicationUser? ReviewedByUser { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
