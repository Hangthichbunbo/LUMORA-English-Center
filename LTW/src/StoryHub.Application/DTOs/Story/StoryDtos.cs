using StoryHub.Domain.Enums;

namespace StoryHub.Application.DTOs.Story;

public class StoryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? AlternativeTitle { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorSlug { get; set; } = string.Empty;
    public StoryStatus Status { get; set; }
    public PublishStatus PublishStatus { get; set; }
    public bool IsFeatured { get; set; }
    public long ViewCount { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int ChapterCount { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public List<string> Categories { get; set; } = [];
}

public class StoryDetailDto : StoryDto
{
    public string? RemovalReason { get; set; }
    public DateTimeOffset? RemovedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public bool IsFollowedByCurrentUser { get; set; }
    public int? CurrentUserRating { get; set; }
    public int? LastReadChapterId { get; set; }
    public decimal? LastReadProgress { get; set; }
    public List<CategorySummaryDto> CategoryDetails { get; set; } = [];
}

public class CategorySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class CreateStoryDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? AlternativeTitle { get; set; }
    public string? Description { get; set; }
    public string? CoverImageUrl { get; set; }
    public int AuthorId { get; set; }
    public string? AuthorName { get; set; }
    public StoryStatus Status { get; set; } = StoryStatus.Draft;
    public PublishStatus PublishStatus { get; set; } = PublishStatus.Draft;
    public bool IsFeatured { get; set; }
    public List<int> CategoryIds { get; set; } = [];
}

public class UpdateStoryDto : CreateStoryDto
{
    public int Id { get; set; }
    public byte[] RowVersion { get; set; } = [];
}

public class RemoveStoryDto
{
    public int StoryId { get; set; }
    public string RemovalReason { get; set; } = string.Empty;
}

public class StoryFilterDto
{
    public string? SearchTerm { get; set; }
    public string? CategorySlug { get; set; }
    public string? AuthorSlug { get; set; }
    public StoryStatus? Status { get; set; }
    public string? SortBy { get; set; } // "updated", "views", "rating", "chapters", "title"
    public bool SortDescending { get; set; } = true;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
