using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Chapter;
using StoryHub.Domain.Entities;
using StoryHub.Domain.Enums;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class ChapterService : IChapterService
{
    private readonly StoryHubDbContext _context;
    private readonly IHtmlSanitizationService _htmlSanitizer;
    private readonly IViewCountBufferService _viewCountBufferService;
    private readonly ICurrentUserService _currentUserService;

    public ChapterService(
        StoryHubDbContext context,
        IHtmlSanitizationService htmlSanitizer,
        IViewCountBufferService viewCountBufferService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _htmlSanitizer = htmlSanitizer;
        _viewCountBufferService = viewCountBufferService;
        _currentUserService = currentUserService;
    }

    public async Task<List<ChapterDto>> GetChaptersByStoryIdAsync(int storyId, CancellationToken cancellationToken = default)
    {
        return await _context.Chapters
            .AsNoTracking()
            .Where(c => c.StoryId == storyId && c.PublishStatus == PublishStatus.Published)
            .OrderBy(c => c.ChapterNumber)
            .Select(c => new ChapterDto
            {
                Id = c.Id,
                StoryId = c.StoryId,
                Title = c.Title,
                Slug = c.Slug,
                ChapterNumber = c.ChapterNumber,
                ViewCount = c.ViewCount,
                PublishedAt = c.PublishedAt,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ChapterDetailDto?> GetChapterForReadingAsync(
        string storySlug,
        decimal chapterNumber,
        string chapterSlug,
        CancellationToken cancellationToken = default)
    {
        var story = await _context.Stories
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Slug == storySlug &&
                                      s.PublishStatus == PublishStatus.Published &&
                                      s.Status != StoryStatus.Hidden &&
                                      s.Status != StoryStatus.Removed &&
                                      s.Status != StoryStatus.Draft, cancellationToken);

        if (story == null) return null;

        var chapter = await _context.Chapters
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.StoryId == story.Id && c.ChapterNumber == chapterNumber && c.PublishStatus == PublishStatus.Published, cancellationToken);

        if (chapter == null && !string.IsNullOrWhiteSpace(chapterSlug))
        {
            chapter = await _context.Chapters
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.StoryId == story.Id && c.Slug == chapterSlug && c.PublishStatus == PublishStatus.Published, cancellationToken);
        }

        if (chapter == null) return null;

        // Buffer view count safely asynchronously
        await _viewCountBufferService.EnqueueViewAsync(story.Id, chapter.Id);

        // Previous and Next chapters
        var prevChapter = await _context.Chapters
            .AsNoTracking()
            .Where(c => c.StoryId == story.Id && c.ChapterNumber < chapterNumber && c.PublishStatus == PublishStatus.Published)
            .OrderByDescending(c => c.ChapterNumber)
            .Select(c => new { c.Id, c.ChapterNumber, c.Slug })
            .FirstOrDefaultAsync(cancellationToken);

        var nextChapter = await _context.Chapters
            .AsNoTracking()
            .Where(c => c.StoryId == story.Id && c.ChapterNumber > chapterNumber && c.PublishStatus == PublishStatus.Published)
            .OrderBy(c => c.ChapterNumber)
            .Select(c => new { c.Id, c.ChapterNumber, c.Slug })
            .FirstOrDefaultAsync(cancellationToken);

        return new ChapterDetailDto
        {
            Id = chapter.Id,
            StoryId = story.Id,
            StoryTitle = story.Title,
            StorySlug = story.Slug,
            Title = chapter.Title,
            Slug = chapter.Slug,
            ChapterNumber = chapter.ChapterNumber,
            Content = _htmlSanitizer.SanitizeHtml(chapter.Content),
            ViewCount = chapter.ViewCount,
            PublishedAt = chapter.PublishedAt,
            CreatedAt = chapter.CreatedAt,
            PrevChapterId = prevChapter?.Id,
            PrevChapterNumber = prevChapter?.ChapterNumber,
            PrevChapterSlug = prevChapter?.Slug,
            NextChapterId = nextChapter?.Id,
            NextChapterNumber = nextChapter?.ChapterNumber,
            NextChapterSlug = nextChapter?.Slug
        };
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền quản lý chương truyện.");
        }
    }

    public async Task<UpdateChapterDto?> GetChapterForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var chapter = await _context.Chapters
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (chapter == null) return null;

        return new UpdateChapterDto
        {
            Id = chapter.Id,
            StoryId = chapter.StoryId,
            ChapterNumber = chapter.ChapterNumber,
            Title = chapter.Title,
            Slug = chapter.Slug,
            Content = chapter.Content,
            PublishStatus = chapter.PublishStatus,
            RowVersion = chapter.RowVersion
        };
    }

    public async Task<ServiceResult<int>> CreateChapterAsync(CreateChapterDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var numberExists = await _context.Chapters
            .IgnoreQueryFilters()
            .AnyAsync(c => c.StoryId == dto.StoryId && c.ChapterNumber == dto.ChapterNumber && !c.IsDeleted, cancellationToken);

        if (numberExists)
        {
            return ServiceResult<int>.Failure($"Chương số {dto.ChapterNumber} đã tồn tại trong truyện này.");
        }

        var slugExists = await _context.Chapters
            .IgnoreQueryFilters()
            .AnyAsync(c => c.StoryId == dto.StoryId && c.Slug == dto.Slug && !c.IsDeleted, cancellationToken);

        if (slugExists)
        {
            return ServiceResult<int>.Failure("Đường dẫn (Slug) của chương đã bị trùng trong truyện này.");
        }

        var chapter = new Chapter
        {
            StoryId = dto.StoryId,
            ChapterNumber = dto.ChapterNumber,
            Title = dto.Title.Trim(),
            Slug = dto.Slug.Trim().ToLowerInvariant(),
            Content = _htmlSanitizer.SanitizeHtml(dto.Content),
            PublishStatus = dto.PublishStatus,
            PublishedAt = dto.PublishStatus == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Chapters.Add(chapter);

        // Update story UpdatedAt timestamp
        var story = await _context.Stories.FindAsync([dto.StoryId], cancellationToken);
        if (story != null) story.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult<int>.Success(chapter.Id, "Thêm chương mới thành công.");
    }

    public async Task<ServiceResult> UpdateChapterAsync(UpdateChapterDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var chapter = await _context.Chapters
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted, cancellationToken);

        if (chapter == null) return ServiceResult.Failure("Không tìm thấy chương cần sửa.");

        var numberConflict = await _context.Chapters
            .IgnoreQueryFilters()
            .AnyAsync(c => c.StoryId == dto.StoryId && c.ChapterNumber == dto.ChapterNumber && c.Id != dto.Id && !c.IsDeleted, cancellationToken);

        if (numberConflict) return ServiceResult.Failure($"Số chương {dto.ChapterNumber} đã tồn tại trong truyện này.");

        _context.Entry(chapter).Property(c => c.RowVersion).OriginalValue = dto.RowVersion;

        chapter.Title = dto.Title.Trim();
        chapter.Slug = dto.Slug.Trim().ToLowerInvariant();
        chapter.ChapterNumber = dto.ChapterNumber;
        chapter.Content = _htmlSanitizer.SanitizeHtml(dto.Content);
        chapter.PublishStatus = dto.PublishStatus;
        chapter.UpdatedAt = DateTimeOffset.UtcNow;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Cập nhật chương thành công.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("Dữ liệu chương đã bị thay đổi bởi người khác. Vui lòng tải lại trang.");
        }
    }

    public async Task<ServiceResult> DeleteChapterAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var chapter = await _context.Chapters
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);

        if (chapter == null) return ServiceResult.Failure("Không tìm thấy chương.");

        var now = DateTimeOffset.UtcNow;
        chapter.IsDeleted = true;
        chapter.DeletedAt = now;
        chapter.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã chuyển chương vào thùng rác. Chương sẽ tự động xóa vĩnh viễn sau 7 ngày.");
    }
}
