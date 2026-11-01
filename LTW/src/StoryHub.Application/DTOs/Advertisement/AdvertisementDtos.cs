using StoryHub.Domain.Enums;

namespace StoryHub.Application.DTOs.Advertisement;

public class AdvertisementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AdType AdType { get; set; }
    public AdPlacement Placement { get; set; }
    public string? ImageUrl { get; set; }
    public string? DestinationUrl { get; set; }
    public string? HtmlContent { get; set; }
    public bool OpenInNewTab { get; set; }
    public int Priority { get; set; }
}

public class CreateOrUpdateAdDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AdType AdType { get; set; }
    public AdPlacement Placement { get; set; }
    public string? ImageUrl { get; set; }
    public string? DestinationUrl { get; set; }
    public string? HtmlContent { get; set; }
    public DateTimeOffset StartAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset EndAt { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);
    public int Priority { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public bool OpenInNewTab { get; set; } = true;
    public int? TargetStoryId { get; set; }
    public string? TargetCategorySlug { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
