using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Comment;
using StoryHub.Domain.Enums;

namespace StoryHub.Application.Interfaces;

public interface ICommentService
{
    // Public & User actions
    Task<PaginatedList<CommentDto>> GetStoryCommentsAsync(int storyId, int? chapterId, int pageIndex = 1, int pageSize = 10, CancellationToken cancellationToken = default);
    Task<ServiceResult<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateCommentAsync(UpdateCommentDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default);
    Task<ServiceResult> ReportCommentAsync(ReportCommentDto dto, CancellationToken cancellationToken = default);

    // Moderation (Moderator & Admin)
    Task<PaginatedList<CommentDto>> GetAdminCommentsPaginatedAsync(CommentStatus? status, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<PaginatedList<CommentReportDto>> GetCommentReportsPaginatedAsync(ReportStatus? status, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ServiceResult> ModerateCommentAsync(int commentId, CommentStatus status, CancellationToken cancellationToken = default);
    Task<ServiceResult> ReviewReportAsync(int reportId, ReportStatus status, CancellationToken cancellationToken = default);
    Task<ServiceResult> BlockUserCommentAsync(string userId, int days, CancellationToken cancellationToken = default);
}
