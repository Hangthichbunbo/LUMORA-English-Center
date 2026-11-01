using StoryHub.Domain.Enums;

namespace StoryHub.Application.DTOs.Chapter;

public class ChapterDto
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal ChapterNumber { get; set; }
    public string FormattedChapterNumber => ChapterNumber % 1 == 0 ? ((int)ChapterNumber).ToString() : ChapterNumber.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture);
    public PublishStatus PublishStatus { get; set; }
    public long ViewCount { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ChapterDetailDto : ChapterDto
{
    public string Content { get; set; } = string.Empty;
    public string StoryTitle { get; set; } = string.Empty;
    public string StorySlug { get; set; } = string.Empty;

    // Navigation buttons
    public int? PrevChapterId { get; set; }
    public decimal? PrevChapterNumber { get; set; }
    public string? FormattedPrevChapterNumber => PrevChapterNumber.HasValue ? (PrevChapterNumber.Value % 1 == 0 ? ((int)PrevChapterNumber.Value).ToString() : PrevChapterNumber.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)) : null;
    public string? PrevChapterSlug { get; set; }

    public int? NextChapterId { get; set; }
    public decimal? NextChapterNumber { get; set; }
    public string? FormattedNextChapterNumber => NextChapterNumber.HasValue ? (NextChapterNumber.Value % 1 == 0 ? ((int)NextChapterNumber.Value).ToString() : NextChapterNumber.Value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)) : null;
    public string? NextChapterSlug { get; set; }
}

public class CreateChapterDto
{
    public int StoryId { get; set; }
    public decimal ChapterNumber { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public PublishStatus PublishStatus { get; set; } = PublishStatus.Published;
}

public class UpdateChapterDto : CreateChapterDto
{
    public int Id { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
