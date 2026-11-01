using StoryHub.Domain.Common;

namespace StoryHub.Domain.Entities;

public class Rating : BaseEntity, IAuditableEntity
{
    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int Score { get; set; } // 1 - 5

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}
