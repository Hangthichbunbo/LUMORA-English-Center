using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.Common.Options;
using StoryHub.Application.DTOs.Comment;
using StoryHub.Domain.Entities;
using StoryHub.Domain.Enums;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly SecuritySettings _securitySettings;

    public CommentService(
        StoryHubDbContext context,
        ICurrentUserService currentUserService,
        IOptions<SecuritySettings> securitySettings)
    {
        _context = context;
        _currentUserService = currentUserService;
        _securitySettings = securitySettings.Value;
    }

    public async Task<PaginatedList<CommentDto>> GetStoryCommentsAsync(
        int storyId,
        int? chapterId,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        var isModOrAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Moderator");

        // 1. Query Root Comments
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.StoryId == storyId && c.ParentCommentId == null && !c.IsDeleted);

        if (chapterId.HasValue)
        {
            query = query.Where(c => c.ChapterId == chapterId.Value);
        }

        // Guests and normal users only see Visible comments
        if (!isModOrAdmin)
        {
            query = query.Where(c => c.Status == CommentStatus.Visible || c.UserId == currentUserId);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var rootComments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.StoryId,
                c.ChapterId,
                ChapterTitle = c.Chapter != null ? c.Chapter.Title : null,
                c.UserId,
                UserDisplayName = c.User.DisplayName,
                UserAvatarUrl = c.User.AvatarUrl,
                c.ParentCommentId,
                c.Content,
                c.Status,
                c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var rootIds = rootComments.Select(c => c.Id).ToList();

        // 2. Query Replies for these root comments
        var repliesQuery = _context.Comments
            .AsNoTracking()
            .Where(c => c.ParentCommentId != null && rootIds.Contains(c.ParentCommentId.Value) && !c.IsDeleted);

        if (!isModOrAdmin)
        {
            repliesQuery = repliesQuery.Where(c => c.Status == CommentStatus.Visible || c.UserId == currentUserId);
        }

        var replies = await repliesQuery
            .OrderBy(c => c.CreatedAt)
            .Select(c => new
            {
                c.Id,
                c.StoryId,
                c.ChapterId,
                ChapterTitle = c.Chapter != null ? c.Chapter.Title : null,
                c.UserId,
                UserDisplayName = c.User.DisplayName,
                UserAvatarUrl = c.User.AvatarUrl,
                c.ParentCommentId,
                c.Content,
                c.Status,
                c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var editWindow = TimeSpan.FromMinutes(_securitySettings.CommentEditWindowMinutes);
        var now = DateTimeOffset.UtcNow;

        var replyMap = replies
            .GroupBy(r => r.ParentCommentId!.Value)
            .ToDictionary(
                g => g.Key,
                g => g.Select(r => new CommentDto
                {
                    Id = r.Id,
                    StoryId = r.StoryId,
                    ChapterId = r.ChapterId,
                    ChapterTitle = r.ChapterTitle,
                    UserId = r.UserId,
                    UserDisplayName = r.UserDisplayName,
                    UserAvatarUrl = r.UserAvatarUrl,
                    ParentCommentId = r.ParentCommentId,
                    Content = r.Content,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    CanEdit = isModOrAdmin || (r.UserId == currentUserId && (now - r.CreatedAt) <= editWindow),
                    CanDelete = isModOrAdmin || (r.UserId == currentUserId && (now - r.CreatedAt) <= editWindow)
                }).ToList()
            );

        var items = rootComments.Select(r => new CommentDto
        {
            Id = r.Id,
            StoryId = r.StoryId,
            ChapterId = r.ChapterId,
            ChapterTitle = r.ChapterTitle,
            UserId = r.UserId,
            UserDisplayName = r.UserDisplayName,
            UserAvatarUrl = r.UserAvatarUrl,
            ParentCommentId = null,
            Content = r.Content,
            Status = r.Status,
            CreatedAt = r.CreatedAt,
            CanEdit = isModOrAdmin || (r.UserId == currentUserId && (now - r.CreatedAt) <= editWindow),
            CanDelete = isModOrAdmin || (r.UserId == currentUserId && (now - r.CreatedAt) <= editWindow),
            Replies = replyMap.GetValueOrDefault(r.Id) ?? []
        }).ToList();

        return new PaginatedList<CommentDto>(items, totalCount, pageIndex, pageSize);
    }

    public async Task<ServiceResult<CommentDto>> CreateCommentAsync(CreateCommentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Vui lòng đăng nhập để bình luận.");
        }

        var userId = _currentUserService.UserId;
        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new ForbiddenAccessException("Tài khoản của bạn đã bị vô hiệu hóa.");
        }

        if (user.CommentBlockedUntil.HasValue && user.CommentBlockedUntil.Value > DateTimeOffset.UtcNow)
        {
            return ServiceResult<CommentDto>.Failure($"Bạn đã bị khóa quyền bình luận cho đến {user.CommentBlockedUntil.Value:dd/MM/yyyy HH:mm}.");
        }

        // Anti-spam rate limiting: cannot post faster than 10 seconds
        var recentComment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (recentComment != null && (DateTimeOffset.UtcNow - recentComment.CreatedAt).TotalSeconds < 10)
        {
            return ServiceResult<CommentDto>.Failure("Bạn thao tác quá nhanh. Vui lòng đợi 10 giây trước khi đăng bình luận tiếp theo.");
        }

        // Flatten replies: Ensure maximum 2 levels
        int? effectiveParentId = dto.ParentCommentId;
        if (effectiveParentId.HasValue)
        {
            var parent = await _context.Comments.FindAsync([effectiveParentId.Value], cancellationToken);
            if (parent != null && parent.ParentCommentId.HasValue)
            {
                // Reply to a reply -> point directly to the root comment!
                effectiveParentId = parent.ParentCommentId.Value;
            }
        }

        // Encode plain text content to neutralize XSS
        var encodedContent = HtmlEncoder.Default.Encode(dto.Content.Trim());

        var comment = new Comment
        {
            StoryId = dto.StoryId,
            ChapterId = dto.ChapterId,
            UserId = userId,
            ParentCommentId = effectiveParentId,
            Content = encodedContent,
            Status = CommentStatus.Visible,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var commentDto = new CommentDto
        {
            Id = comment.Id,
            StoryId = comment.StoryId,
            ChapterId = comment.ChapterId,
            UserId = user.Id,
            UserDisplayName = user.DisplayName,
            UserAvatarUrl = user.AvatarUrl,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            Status = comment.Status,
            CreatedAt = comment.CreatedAt,
            CanEdit = true,
            CanDelete = true
        };

        return ServiceResult<CommentDto>.Success(commentDto, "Bình luận thành công.");
    }

    public async Task<ServiceResult> UpdateCommentAsync(UpdateCommentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException();
        }

        var comment = await _context.Comments.FindAsync([dto.CommentId], cancellationToken);
        if (comment == null || comment.IsDeleted)
        {
            return ServiceResult.Failure("Không tìm thấy bình luận.");
        }

        var isModOrAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Moderator");
        var isOwner = comment.UserId == _currentUserService.UserId;

        if (!isModOrAdmin)
        {
            if (!isOwner) throw new ForbiddenAccessException("Bạn không có quyền sửa bình luận của người khác.");

            var editWindow = TimeSpan.FromMinutes(_securitySettings.CommentEditWindowMinutes);
            if (DateTimeOffset.UtcNow - comment.CreatedAt > editWindow)
            {
                return ServiceResult.Failure($"Đã hết thời gian cho phép chỉnh sửa bình luận ({_securitySettings.CommentEditWindowMinutes} phút).");
            }
        }

        comment.Content = HtmlEncoder.Default.Encode(dto.Content.Trim());
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Cập nhật bình luận thành công.");
    }

    public async Task<ServiceResult> DeleteCommentAsync(int commentId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException();
        }

        var comment = await _context.Comments.FindAsync([commentId], cancellationToken);
        if (comment == null || comment.IsDeleted) return ServiceResult.Failure("Không tìm thấy bình luận.");

        var isModOrAdmin = _currentUserService.IsInRole("Admin") || _currentUserService.IsInRole("Moderator");
        var isOwner = comment.UserId == _currentUserService.UserId;

        if (!isModOrAdmin)
        {
            if (!isOwner)
            {
                throw new ForbiddenAccessException("Bạn không có quyền xóa bình luận của người khác.");
            }

            var editWindow = TimeSpan.FromMinutes(_securitySettings.CommentEditWindowMinutes);
            if (DateTimeOffset.UtcNow - comment.CreatedAt > editWindow)
            {
                return ServiceResult.Failure($"Đã hết thời gian cho phép xóa bình luận ({_securitySettings.CommentEditWindowMinutes} phút).");
            }
        }

        comment.IsDeleted = true;
        comment.DeletedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Xóa bình luận thành công.");
    }

    public async Task<ServiceResult> ReportCommentAsync(ReportCommentDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Vui lòng đăng nhập để báo cáo bình luận.");
        }

        var comment = await _context.Comments.FindAsync([dto.CommentId], cancellationToken);
        if (comment == null || comment.IsDeleted) return ServiceResult.Failure("Bình luận không tồn tại.");

        var report = new CommentReport
        {
            CommentId = dto.CommentId,
            ReportedByUserId = _currentUserService.UserId,
            Reason = dto.Reason.Trim(),
            Description = dto.Description?.Trim(),
            Status = ReportStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.CommentReports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Gửi báo cáo vi phạm thành công. Ban quản trị sẽ kiểm tra sớm nhất.");
    }

    // ==========================================
    // MODERATOR & ADMIN MODERATION
    // ==========================================

    private void EnsureModeratorOrAdmin()
    {
        if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Moderator"))
        {
            throw new ForbiddenAccessException("Bạn không có quyền thực hiện kiểm duyệt.");
        }
    }

    public async Task<PaginatedList<CommentDto>> GetAdminCommentsPaginatedAsync(
        CommentStatus? status,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureModeratorOrAdmin();

        var query = _context.Comments
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        var projected = query.OrderByDescending(c => c.CreatedAt).Select(c => new CommentDto
        {
            Id = c.Id,
            StoryId = c.StoryId,
            ChapterId = c.ChapterId,
            ChapterTitle = c.Chapter != null ? c.Chapter.Title : null,
            UserId = c.UserId,
            UserDisplayName = c.User.DisplayName,
            UserAvatarUrl = c.User.AvatarUrl,
            Content = c.Content,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            CanEdit = true,
            CanDelete = true
        });

        return await PaginatedList<CommentDto>.CreateAsync(projected, pageIndex, pageSize, cancellationToken);
    }

    public async Task<PaginatedList<CommentReportDto>> GetCommentReportsPaginatedAsync(
        ReportStatus? status,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureModeratorOrAdmin();

        var query = _context.CommentReports.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var projected = query.OrderByDescending(r => r.CreatedAt).Select(r => new CommentReportDto
        {
            Id = r.Id,
            CommentId = r.CommentId,
            CommentContent = r.Comment.Content,
            ReportedByUserId = r.ReportedByUserId,
            ReportedByUserName = r.ReportedByUser.DisplayName,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            ReviewedByUserName = r.ReviewedByUser != null ? r.ReviewedByUser.DisplayName : null,
            ReviewedAt = r.ReviewedAt,
            CreatedAt = r.CreatedAt
        });

        return await PaginatedList<CommentReportDto>.CreateAsync(projected, pageIndex, pageSize, cancellationToken);
    }

    public async Task<ServiceResult> ModerateCommentAsync(int commentId, CommentStatus status, CancellationToken cancellationToken = default)
    {
        EnsureModeratorOrAdmin();

        var comment = await _context.Comments.FindAsync([commentId], cancellationToken);
        if (comment == null) return ServiceResult.Failure("Không tìm thấy bình luận.");

        comment.Status = status;
        comment.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success($"Đã đổi trạng thái bình luận sang '{status}'.");
    }

    public async Task<ServiceResult> ReviewReportAsync(int reportId, ReportStatus status, CancellationToken cancellationToken = default)
    {
        EnsureModeratorOrAdmin();

        var report = await _context.CommentReports.FindAsync([reportId], cancellationToken);
        if (report == null) return ServiceResult.Failure("Không tìm thấy báo cáo.");

        report.Status = status;
        report.ReviewedByUserId = _currentUserService.UserId;
        report.ReviewedAt = DateTimeOffset.UtcNow;

        // If approved -> hide the offending comment
        if (status == ReportStatus.Approved)
        {
            var comment = await _context.Comments.FindAsync([report.CommentId], cancellationToken);
            if (comment != null)
            {
                comment.Status = CommentStatus.Hidden;
                comment.UpdatedAt = DateTimeOffset.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã xử lý báo cáo vi phạm.");
    }

    public async Task<ServiceResult> BlockUserCommentAsync(string userId, int days, CancellationToken cancellationToken = default)
    {
        EnsureModeratorOrAdmin();

        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user == null) return ServiceResult.Failure("Không tìm thấy người dùng.");

        user.CommentBlockedUntil = days > 0 ? DateTimeOffset.UtcNow.AddDays(days) : DateTimeOffset.UtcNow.AddYears(100);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success($"Đã khóa tính năng bình luận của người dùng {user.DisplayName} trong {days} ngày.");
    }
}
