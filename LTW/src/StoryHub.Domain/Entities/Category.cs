using StoryHub.Domain.Common;

namespace StoryHub.Domain.Entities;

public class Category : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public virtual ICollection<StoryCategory> StoryCategories { get; set; } = new List<StoryCategory>();
}
