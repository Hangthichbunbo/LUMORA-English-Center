using StoryHub.Domain.Common;
using StoryHub.Domain.Enums;

namespace StoryHub.Domain.Entities;

public class Comment : BaseEntity, IAuditableEntity, ISoftDeletable
{
    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public int? ChapterId { get; set; }
    public virtual Chapter? Chapter { get; set; }

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int? ParentCommentId { get; set; }
    public virtual Comment? ParentComment { get; set; }

    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; } = CommentStatus.Visible;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public virtual ICollection<Comment> Replies { get; set; } = new List<Comment>();
    public virtual ICollection<CommentReport> Reports { get; set; } = new List<CommentReport>();
}
