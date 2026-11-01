namespace StoryHub.Application.DTOs.Trash;

public class TrashSummaryDto
{
    public int DeletedStoriesCount { get; set; }
    public int DeletedChaptersCount { get; set; }
    public int ExpiringSoonCount { get; set; }
}

public class TrashStoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int ChapterCount { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int DaysRemaining { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public bool IsExpired { get; set; }
}

public class TrashChapterDto
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string StoryTitle { get; set; } = string.Empty;
    public string StorySlug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal ChapterNumber { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public int DaysRemaining { get; set; }
    public TimeSpan TimeRemaining { get; set; }
    public bool IsExpired { get; set; }
}

