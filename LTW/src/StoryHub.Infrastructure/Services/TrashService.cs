using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Trash;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class TrashService : ITrashService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<TrashService> _logger;

    public TrashService(
        StoryHubDbContext context,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        ILogger<TrashService> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền truy cập Thùng rác.");
        }
    }

    public async Task<TrashSummaryDto> GetTrashSummaryAsync(CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var deletedStoriesCount = await _context.Stories
            .IgnoreQueryFilters()
            .CountAsync(s => s.IsDeleted, cancellationToken);

        var deletedChaptersCount = await _context.Chapters
            .IgnoreQueryFilters()
            .CountAsync(c => c.IsDeleted && !c.Story.IsDeleted, cancellationToken);

        var threshold = DateTimeOffset.UtcNow.AddDays(-5); // Within 2 days of 7-day expiration
        var expiringStories = await _context.Stories
            .IgnoreQueryFilters()
            .CountAsync(s => s.IsDeleted && s.DeletedAt != null && s.DeletedAt <= threshold, cancellationToken);

        var expiringChapters = await _context.Chapters
            .IgnoreQueryFilters()
            .CountAsync(c => c.IsDeleted && !c.Story.IsDeleted && c.DeletedAt != null && c.DeletedAt <= threshold, cancellationToken);

        return new TrashSummaryDto
        {
            DeletedStoriesCount = deletedStoriesCount,
            DeletedChaptersCount = deletedChaptersCount,
            ExpiringSoonCount = expiringStories + expiringChapters
        };
    }

    public async Task<PaginatedList<TrashStoryDto>> GetDeletedStoriesAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var query = _context.Stories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.Author)
            .Include(s => s.Chapters)
            .Where(s => s.IsDeleted)
            .OrderByDescending(s => s.DeletedAt ?? s.UpdatedAt ?? s.CreatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Slug,
                s.CoverImageUrl,
                AuthorName = s.Author.Name,
                ChapterCount = s.Chapters.Count,
                DeletedAt = s.DeletedAt ?? s.UpdatedAt ?? s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var dtos = items.Select(s =>
        {
            var expiresAt = s.DeletedAt.AddDays(7);
            var timeRemaining = expiresAt > now ? expiresAt - now : TimeSpan.Zero;
            var daysRemaining = (int)Math.Ceiling(timeRemaining.TotalDays);
            return new TrashStoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.AuthorName,
                ChapterCount = s.ChapterCount,
                DeletedAt = s.DeletedAt,
                DaysRemaining = daysRemaining,
                TimeRemaining = timeRemaining,
                IsExpired = expiresAt <= now
            };
        }).ToList();

        return new PaginatedList<TrashStoryDto>(dtos, count, pageIndex, pageSize);
    }

    public async Task<PaginatedList<TrashChapterDto>> GetDeletedChaptersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var query = _context.Chapters
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(c => c.Story)
            .Where(c => c.IsDeleted && !c.Story.IsDeleted)
            .OrderByDescending(c => c.DeletedAt ?? c.UpdatedAt ?? c.CreatedAt);

        var count = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new
            {
                c.Id,
                c.StoryId,
                StoryTitle = c.Story.Title,
                StorySlug = c.Story.Slug,
                c.Title,
                c.Slug,
                c.ChapterNumber,
                DeletedAt = c.DeletedAt ?? c.UpdatedAt ?? c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var dtos = items.Select(c =>
        {
            var expiresAt = c.DeletedAt.AddDays(7);
            var timeRemaining = expiresAt > now ? expiresAt - now : TimeSpan.Zero;
            var daysRemaining = (int)Math.Ceiling(timeRemaining.TotalDays);
            return new TrashChapterDto
            {
                Id = c.Id,
                StoryId = c.StoryId,
                StoryTitle = c.StoryTitle,
                StorySlug = c.StorySlug,
                Title = c.Title,
                Slug = c.Slug,
                ChapterNumber = c.ChapterNumber,
                DeletedAt = c.DeletedAt,
                DaysRemaining = daysRemaining,
                TimeRemaining = timeRemaining,
                IsExpired = expiresAt <= now
            };
        }).ToList();

        return new PaginatedList<TrashChapterDto>(dtos, count, pageIndex, pageSize);
    }

    public async Task<ServiceResult> RestoreStoryAsync(int storyId, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == storyId && s.IsDeleted, cancellationToken);

        if (story == null)
        {
            return ServiceResult.Failure("Không tìm thấy truyện trong thùng rác.");
        }

        var now = DateTimeOffset.UtcNow;
        story.IsDeleted = false;
        story.DeletedAt = null;
        story.UpdatedAt = now;

        foreach (var chapter in story.Chapters)
        {
            chapter.IsDeleted = false;
            chapter.DeletedAt = null;
            chapter.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin đã khôi phục truyện '{Title}' (ID: {StoryId}) cùng {ChapterCount} chương khỏi thùng rác.", story.Title, story.Id, story.Chapters.Count);

        return ServiceResult.Success($"Đã khôi phục truyện '{story.Title}' thành công.");
    }

    public async Task<ServiceResult> RestoreChapterAsync(int chapterId, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var chapter = await _context.Chapters
            .IgnoreQueryFilters()
            .Include(c => c.Story)
            .FirstOrDefaultAsync(c => c.Id == chapterId && c.IsDeleted, cancellationToken);

        if (chapter == null)
        {
            return ServiceResult.Failure("Không tìm thấy chương trong thùng rác.");
        }

        var now = DateTimeOffset.UtcNow;
        if (chapter.Story != null && chapter.Story.IsDeleted)
        {
            chapter.Story.IsDeleted = false;
            chapter.Story.DeletedAt = null;
            chapter.Story.UpdatedAt = now;
        }

        chapter.IsDeleted = false;
        chapter.DeletedAt = null;
        chapter.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Admin đã khôi phục chương '{Title}' (ID: {ChapterId}) khỏi thùng rác.", chapter.Title, chapter.Id);

        return ServiceResult.Success($"Đã khôi phục chương '{chapter.Title}' thành công.");
    }

    public async Task<ServiceResult> PermanentlyDeleteStoryAsync(int storyId, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == storyId, cancellationToken);

        if (story == null)
        {
            return ServiceResult.Failure("Không tìm thấy truyện cần xóa vĩnh viễn.");
        }

        var title = story.Title;
        await DeleteStoryInternalAsync(story, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã xóa vĩnh viễn truyện '{Title}' (ID: {StoryId}) và toàn bộ nội dung liên quan.", title, storyId);
        return ServiceResult.Success($"Đã xóa vĩnh viễn truyện '{title}' và toàn bộ nội dung liên quan.");
    }

    public async Task<ServiceResult> PermanentlyDeleteChapterAsync(int chapterId, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var chapter = await _context.Chapters
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == chapterId, cancellationToken);

        if (chapter == null)
        {
            return ServiceResult.Failure("Không tìm thấy chương cần xóa vĩnh viễn.");
        }

        var title = chapter.Title;
        await DeleteChapterInternalAsync(chapter, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Đã xóa vĩnh viễn chương '{Title}' (ID: {ChapterId}) và toàn bộ nội dung liên quan.", title, chapterId);
        return ServiceResult.Success($"Đã xóa vĩnh viễn chương '{title}' và toàn bộ nội dung liên quan.");
    }

    public async Task<int> CleanupExpiredItemsAsync(CancellationToken cancellationToken = default)
    {
        var threshold = DateTimeOffset.UtcNow.AddDays(-7);
        int deletedCount = 0;

        // 1. Expired Stories
        var expiredStories = await _context.Stories
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted && (s.DeletedAt != null ? s.DeletedAt <= threshold : (s.UpdatedAt ?? s.CreatedAt) <= threshold))
            .ToListAsync(cancellationToken);

        foreach (var story in expiredStories)
        {
            try
            {
                await DeleteStoryInternalAsync(story, cancellationToken);
                deletedCount++;
                _logger.LogInformation("Tự động dọn dẹp vĩnh viễn truyện hết hạn 7 ngày: '{Title}' (ID: {Id})", story.Title, story.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tự động dọn dẹp truyện {StoryId}", story.Id);
            }
        }

        // 2. Expired individual Chapters
        var expiredChapters = await _context.Chapters
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted && (c.DeletedAt != null ? c.DeletedAt <= threshold : (c.UpdatedAt ?? c.CreatedAt) <= threshold))
            .ToListAsync(cancellationToken);

        foreach (var ch in expiredChapters)
        {
            try
            {
                await DeleteChapterInternalAsync(ch, cancellationToken);
                deletedCount++;
                _logger.LogInformation("Tự động dọn dẹp vĩnh viễn chương hết hạn 7 ngày: '{Title}' (ID: {Id})", ch.Title, ch.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tự động dọn dẹp chương {ChapterId}", ch.Id);
            }
        }

        if (deletedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return deletedCount;
    }

    private async Task DeleteChapterInternalAsync(Chapter chapter, CancellationToken cancellationToken)
    {
        // 1. Clean up images from chapter content
        if (!string.IsNullOrWhiteSpace(chapter.Content))
        {
            var imgUrls = ExtractImageUrls(chapter.Content);
            foreach (var url in imgUrls)
            {
                await _fileStorageService.DeleteFileAsync(url, cancellationToken);
            }
        }

        // 2. Clean up reading histories for this chapter
        var histories = await _context.ReadingHistories
            .Where(rh => rh.ChapterId == chapter.Id)
            .ToListAsync(cancellationToken);
        if (histories.Count > 0)
        {
            _context.ReadingHistories.RemoveRange(histories);
        }

        // 3. Clean up comments for this chapter
        var comments = await _context.Comments
            .IgnoreQueryFilters()
            .Include(c => c.Replies)
            .Include(c => c.Reports)
            .Where(c => c.ChapterId == chapter.Id)
            .ToListAsync(cancellationToken);

        if (comments.Count > 0)
        {
            foreach (var comment in comments)
            {
                if (comment.Reports.Count > 0)
                {
                    _context.CommentReports.RemoveRange(comment.Reports);
                }
                if (comment.Replies.Count > 0)
                {
                    _context.Comments.RemoveRange(comment.Replies);
                }
            }
            _context.Comments.RemoveRange(comments);
        }

        _context.Chapters.Remove(chapter);
    }

    private async Task DeleteStoryInternalAsync(Story story, CancellationToken cancellationToken)
    {
        // 1. Delete all chapters belonging to the story
        var chapters = await _context.Chapters
            .IgnoreQueryFilters()
            .Where(c => c.StoryId == story.Id)
            .ToListAsync(cancellationToken);

        foreach (var ch in chapters)
        {
            await DeleteChapterInternalAsync(ch, cancellationToken);
        }

        // 2. Delete story-level comments
        var storyComments = await _context.Comments
            .IgnoreQueryFilters()
            .Include(c => c.Replies)
            .Include(c => c.Reports)
            .Where(c => c.StoryId == story.Id)
            .ToListAsync(cancellationToken);

        if (storyComments.Count > 0)
        {
            foreach (var comment in storyComments)
            {
                if (comment.Reports.Count > 0)
                {
                    _context.CommentReports.RemoveRange(comment.Reports);
                }
                if (comment.Replies.Count > 0)
                {
                    _context.Comments.RemoveRange(comment.Replies);
                }
            }
            _context.Comments.RemoveRange(storyComments);
        }

        // 3. Delete reading histories, ratings, followed stories, categories
        var histories = await _context.ReadingHistories.Where(rh => rh.StoryId == story.Id).ToListAsync(cancellationToken);
        if (histories.Count > 0) _context.ReadingHistories.RemoveRange(histories);

        var ratings = await _context.Ratings.Where(r => r.StoryId == story.Id).ToListAsync(cancellationToken);
        if (ratings.Count > 0) _context.Ratings.RemoveRange(ratings);

        var follows = await _context.FollowedStories.Where(f => f.StoryId == story.Id).ToListAsync(cancellationToken);
        if (follows.Count > 0) _context.FollowedStories.RemoveRange(follows);

        var categories = await _context.StoryCategories.Where(sc => sc.StoryId == story.Id).ToListAsync(cancellationToken);
        if (categories.Count > 0) _context.StoryCategories.RemoveRange(categories);

        // 4. Delete story cover file
        if (!string.IsNullOrWhiteSpace(story.CoverImageUrl))
        {
            await _fileStorageService.DeleteFileAsync(story.CoverImageUrl, cancellationToken);
        }

        _context.Stories.Remove(story);
    }

    private static List<string> ExtractImageUrls(string htmlContent)
    {
        var urls = new List<string>();
        if (string.IsNullOrWhiteSpace(htmlContent)) return urls;

        var matches = Regex.Matches(htmlContent, @"<img\s+[^>]*?src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            if (match.Groups.Count > 1)
            {
                var src = match.Groups[1].Value.Trim();
                if (!string.IsNullOrWhiteSpace(src) &&
                    (src.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) ||
                     src.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase) ||
                     src.StartsWith("/app/uploads/", StringComparison.OrdinalIgnoreCase) ||
                     src.StartsWith("//app/uploads/", StringComparison.OrdinalIgnoreCase)))
                {
                    urls.Add(src);
                }
            }
        }
        return urls;
    }
}

