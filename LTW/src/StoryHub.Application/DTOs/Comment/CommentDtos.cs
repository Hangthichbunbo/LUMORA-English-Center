using StoryHub.Domain.Enums;

namespace StoryHub.Application.DTOs.Comment;

public class CommentDto
{
    public int Id { get; set; }
    public int StoryId { get; set; }
    public int? ChapterId { get; set; }
    public string? ChapterTitle { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserDisplayName { get; set; } = string.Empty;
    public string? UserAvatarUrl { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public CommentStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<CommentDto> Replies { get; set; } = [];
}

public class CreateCommentDto
{
    public int StoryId { get; set; }
    public int? ChapterId { get; set; }
    public int? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentDto
{
    public int CommentId { get; set; }
    public string Content { get; set; } = string.Empty;
}

public class ReportCommentDto
{
    public int CommentId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CommentReportDto
{
    public int Id { get; set; }
    public int CommentId { get; set; }
    public string CommentContent { get; set; } = string.Empty;
    public string ReportedByUserId { get; set; } = string.Empty;
    public string ReportedByUserName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ReportStatus Status { get; set; }
    public string? ReviewedByUserName { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
