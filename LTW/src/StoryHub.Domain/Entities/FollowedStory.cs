namespace StoryHub.Domain.Entities;

public class FollowedStory
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
