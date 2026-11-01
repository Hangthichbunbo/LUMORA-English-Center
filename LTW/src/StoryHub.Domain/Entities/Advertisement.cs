using StoryHub.Domain.Common;
using StoryHub.Domain.Enums;

namespace StoryHub.Domain.Entities;

public class Advertisement : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public AdType AdType { get; set; } = AdType.Image;
    public AdPlacement Placement { get; set; } = AdPlacement.Header;

    public string? ImageUrl { get; set; }
    public string? DestinationUrl { get; set; }
    public string? HtmlContent { get; set; }

    public DateTimeOffset StartAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    public int Priority { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public bool OpenInNewTab { get; set; } = true;

    // Optional Targeting
    public int? TargetStoryId { get; set; }
    public string? TargetCategorySlug { get; set; }

    // Metrics
    public long ImpressionCount { get; set; }
    public long ClickCount { get; set; }

    public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
