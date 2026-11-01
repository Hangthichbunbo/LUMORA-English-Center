using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Helpers;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Story;
using StoryHub.Domain.Entities;
using StoryHub.Domain.Enums;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class StoryService : IStoryService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StoryService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PaginatedList<StoryDto>> GetStoriesPaginatedAsync(StoryFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Stories
            .AsNoTracking()
            .Where(s => s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var search = filter.SearchTerm.Trim();
            query = query.Where(s => s.Title.Contains(search) ||
                                     (s.AlternativeTitle != null && s.AlternativeTitle.Contains(search)) ||
                                     s.Author.Name.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(filter.CategorySlug))
        {
            query = query.Where(s => s.StoryCategories.Any(sc => sc.Category.Slug == filter.CategorySlug));
        }

        if (!string.IsNullOrWhiteSpace(filter.AuthorSlug))
        {
            query = query.Where(s => s.Author.Slug == filter.AuthorSlug);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(s => s.Status == filter.Status.Value);
        }

        // Sorting
        query = (filter.SortBy?.ToLowerInvariant(), filter.SortDescending) switch
        {
            ("views", true) => query.OrderByDescending(s => s.ViewCount),
            ("views", false) => query.OrderBy(s => s.ViewCount),
            ("rating", true) => query.OrderByDescending(s => s.AverageRating).ThenByDescending(s => s.RatingCount),
            ("rating", false) => query.OrderBy(s => s.AverageRating),
            ("chapters", true) => query.OrderByDescending(s => s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published)),
            ("chapters", false) => query.OrderBy(s => s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published)),
            ("title", true) => query.OrderByDescending(s => s.Title),
            ("title", false) => query.OrderBy(s => s.Title),
            _ => filter.SortDescending ? query.OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt) : query.OrderBy(s => s.UpdatedAt ?? s.CreatedAt)
        };

        var projected = query.Select(s => new StoryDto
        {
            Id = s.Id,
            Title = s.Title,
            Slug = s.Slug,
            AlternativeTitle = s.AlternativeTitle,
            Description = s.Description,
            CoverImageUrl = s.CoverImageUrl,
            AuthorId = s.AuthorId,
            AuthorName = s.Author.Name,
            AuthorSlug = s.Author.Slug,
            Status = s.Status,
            PublishStatus = s.PublishStatus,
            IsFeatured = s.IsFeatured,
            ViewCount = s.ViewCount,
            AverageRating = s.AverageRating,
            RatingCount = s.RatingCount,
            ChapterCount = s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published),
            CreatedAt = s.CreatedAt,
            PublishedAt = s.PublishedAt,
            UpdatedAt = s.UpdatedAt,
            Categories = s.StoryCategories.Select(sc => sc.Category.Name).ToList()
        });

        return await PaginatedList<StoryDto>.CreateAsync(projected, filter.PageIndex, filter.PageSize, cancellationToken);
    }

    public async Task<StoryDetailDto?> GetStoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var story = await _context.Stories
            .AsNoTracking()
            .Where(s => s.Slug == slug &&
                        s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft)
            .Select(s => new StoryDetailDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                AlternativeTitle = s.AlternativeTitle,
                Description = s.Description,
                CoverImageUrl = s.CoverImageUrl,
                AuthorId = s.AuthorId,
                AuthorName = s.Author.Name,
                AuthorSlug = s.Author.Slug,
                Status = s.Status,
                PublishStatus = s.PublishStatus,
                IsFeatured = s.IsFeatured,
                ViewCount = s.ViewCount,
                AverageRating = s.AverageRating,
                RatingCount = s.RatingCount,
                ChapterCount = s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published),
                CreatedAt = s.CreatedAt,
                PublishedAt = s.PublishedAt,
                UpdatedAt = s.UpdatedAt,
                RowVersion = s.RowVersion,
                Categories = s.StoryCategories.Select(sc => sc.Category.Name).ToList(),
                CategoryDetails = s.StoryCategories.Select(sc => new CategorySummaryDto
                {
                    Id = sc.CategoryId,
                    Name = sc.Category.Name,
                    Slug = sc.Category.Slug
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (story != null && _currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
        {
            var userId = _currentUserService.UserId;
            story.IsFollowedByCurrentUser = await _context.FollowedStories
                .AnyAsync(f => f.StoryId == story.Id && f.UserId == userId, cancellationToken);

            story.CurrentUserRating = await _context.Ratings
                .Where(r => r.StoryId == story.Id && r.UserId == userId)
                .Select(r => (int?)r.Score)
                .FirstOrDefaultAsync(cancellationToken);

            var history = await _context.ReadingHistories
                .Where(h => h.StoryId == story.Id && h.UserId == userId)
                .Select(h => new { h.ChapterId, h.Progress })
                .FirstOrDefaultAsync(cancellationToken);

            if (history != null)
            {
                story.LastReadChapterId = history.ChapterId;
                story.LastReadProgress = history.Progress;
            }
        }

        return story;
    }

    public async Task<List<StoryDto>> GetFeaturedStoriesAsync(int count = 6, CancellationToken cancellationToken = default)
    {
        return await _context.Stories
            .AsNoTracking()
            .Where(s => s.IsFeatured &&
                        s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .Take(count)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                Description = s.Description,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                AuthorSlug = s.Author.Slug,
                Status = s.Status,
                ViewCount = s.ViewCount,
                AverageRating = s.AverageRating,
                RatingCount = s.RatingCount,
                Categories = s.StoryCategories.Select(sc => sc.Category.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoryDto>> GetLatestUpdatedStoriesAsync(int count = 12, CancellationToken cancellationToken = default)
    {
        return await _context.Stories
            .AsNoTracking()
            .Where(s => s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .Take(count)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                AuthorSlug = s.Author.Slug,
                Status = s.Status,
                ViewCount = s.ViewCount,
                ChapterCount = s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published),
                UpdatedAt = s.UpdatedAt ?? s.CreatedAt,
                Categories = s.StoryCategories.Select(sc => sc.Category.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoryDto>> GetTopViewedStoriesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Stories
            .AsNoTracking()
            .Where(s => s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft)
            .OrderByDescending(s => s.ViewCount)
            .Take(count)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                AuthorSlug = s.Author.Slug,
                ViewCount = s.ViewCount,
                AverageRating = s.AverageRating
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoryDto>> GetTopRatedStoriesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Stories
            .AsNoTracking()
            .Where(s => s.PublishStatus == PublishStatus.Published &&
                        s.Status != StoryStatus.Hidden &&
                        s.Status != StoryStatus.Removed &&
                        s.Status != StoryStatus.Draft &&
                        s.RatingCount >= 5)
            .OrderByDescending(s => s.AverageRating)
            .ThenByDescending(s => s.RatingCount)
            .Take(count)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                AverageRating = s.AverageRating,
                RatingCount = s.RatingCount
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<StoryDto>> GetCompletedStoriesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Stories
            .AsNoTracking()
            .Where(s => s.Status == StoryStatus.Completed && s.PublishStatus == PublishStatus.Published)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .Take(count)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                AuthorSlug = s.Author.Slug,
                Status = s.Status,
                ViewCount = s.ViewCount,
                AverageRating = s.AverageRating,
                RatingCount = s.RatingCount,
                ChapterCount = s.Chapters.Count(c => c.PublishStatus == PublishStatus.Published),
                UpdatedAt = s.UpdatedAt ?? s.CreatedAt,
                Categories = s.StoryCategories.Select(sc => sc.Category.Name).ToList()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<ServiceResult<bool>> ToggleFollowStoryAsync(int storyId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return ServiceResult<bool>.Failure("Vui lòng đăng nhập để theo dõi truyện.");
        }

        var userId = _currentUserService.UserId;
        var existing = await _context.FollowedStories
            .FirstOrDefaultAsync(f => f.StoryId == storyId && f.UserId == userId, cancellationToken);

        if (existing != null)
        {
            _context.FollowedStories.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult<bool>.Success(false, "Đã bỏ theo dõi truyện.");
        }

        _context.FollowedStories.Add(new FollowedStory
        {
            UserId = userId,
            StoryId = storyId,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult<bool>.Success(true, "Theo dõi truyện thành công.");
    }

    public async Task<PaginatedList<StoryDto>> GetFollowedStoriesAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Vui lòng đăng nhập để xem danh sách truyện đang theo dõi.");
        }

        var userId = _currentUserService.UserId;
        var query = _context.FollowedStories
            .AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new StoryDto
            {
                Id = f.Story.Id,
                Title = f.Story.Title,
                Slug = f.Story.Slug,
                CoverImageUrl = f.Story.CoverImageUrl,
                AuthorName = f.Story.Author.Name,
                AuthorSlug = f.Story.Author.Slug,
                Status = f.Story.Status,
                ChapterCount = f.Story.Chapters.Count(c => c.PublishStatus == PublishStatus.Published),
                UpdatedAt = f.Story.UpdatedAt ?? f.Story.CreatedAt
            });

        return await PaginatedList<StoryDto>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
    }

    public async Task<ServiceResult> SaveReadingProgressAsync(int storyId, int chapterId, decimal progress, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            return ServiceResult.Success(); // Silent return for guests
        }

        var userId = _currentUserService.UserId;
        var history = await _context.ReadingHistories
            .FirstOrDefaultAsync(h => h.UserId == userId && h.StoryId == storyId, cancellationToken);

        if (history == null)
        {
            _context.ReadingHistories.Add(new ReadingHistory
            {
                UserId = userId,
                StoryId = storyId,
                ChapterId = chapterId,
                Progress = progress,
                LastReadAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            history.ChapterId = chapterId;
            history.Progress = progress;
            history.LastReadAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }

    public async Task<PaginatedList<StoryDto>> GetReadingHistoriesAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException();
        }

        var userId = _currentUserService.UserId;
        var query = _context.ReadingHistories
            .AsNoTracking()
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.LastReadAt)
            .Select(h => new StoryDto
            {
                Id = h.Story.Id,
                Title = h.Story.Title,
                Slug = h.Story.Slug,
                CoverImageUrl = h.Story.CoverImageUrl,
                AuthorName = h.Story.Author.Name,
                Status = h.Story.Status,
                UpdatedAt = h.LastReadAt
            });

        return await PaginatedList<StoryDto>.CreateAsync(query, pageIndex, pageSize, cancellationToken);
    }

    // ==========================================
    // ADMIN OPERATIONS (STRICTLY ROLE: ADMIN ONLY)
    // ==========================================

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền thực hiện thao tác này.");
        }
    }

    public async Task<PaginatedList<StoryDto>> GetAdminStoriesPaginatedAsync(
        string? search,
        StoryStatus? status,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        // Admin queries ignore soft delete or public publish filter
        var query = _context.Stories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(s => s.Title.Contains(term) || s.Author.Name.Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(s => s.Status == status.Value);
        }

        var projected = query.OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt).Select(s => new StoryDto
        {
            Id = s.Id,
            Title = s.Title,
            Slug = s.Slug,
            CoverImageUrl = s.CoverImageUrl,
            AuthorName = s.Author.Name,
            Status = s.Status,
            PublishStatus = s.PublishStatus,
            ViewCount = s.ViewCount,
            AverageRating = s.AverageRating,
            RatingCount = s.RatingCount,
            ChapterCount = s.Chapters.Count(),
            PublishedAt = s.PublishedAt,
            UpdatedAt = s.UpdatedAt ?? s.CreatedAt
        });

        return await PaginatedList<StoryDto>.CreateAsync(projected, pageIndex, pageSize, cancellationToken);
    }

    public async Task<UpdateStoryDto?> GetStoryForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(s => s.Author)
            .Include(s => s.StoryCategories)
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (story == null) return null;

        return new UpdateStoryDto
        {
            Id = story.Id,
            Title = story.Title,
            Slug = story.Slug,
            AlternativeTitle = story.AlternativeTitle,
            Description = story.Description,
            CoverImageUrl = NormalizeCoverImageUrl(story.CoverImageUrl),
            AuthorId = story.AuthorId,
            AuthorName = story.Author?.Name,
            Status = story.Status,
            PublishStatus = story.PublishStatus,
            IsFeatured = story.IsFeatured,
            RowVersion = story.RowVersion,
            CategoryIds = story.StoryCategories.Select(sc => sc.CategoryId).ToList()
        };
    }

    private async Task<int> ResolveAuthorIdAsync(int authorId, string? authorName, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(authorName))
        {
            var trimmedName = authorName.Trim();
            var cleanName = System.Text.RegularExpressions.Regex.Replace(trimmedName, @"<script[^>]*>[\s\S]*?</script>", string.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"<[^>]+>", string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(cleanName)) cleanName = trimmedName;

            var existing = await _context.Authors
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Name.ToLower() == cleanName.ToLower() && !a.IsDeleted, cancellationToken);

            if (existing != null)
            {
                return existing.Id;
            }

            // Create new Author
            var baseSlug = GenerateSlug(cleanName);
            if (string.IsNullOrWhiteSpace(baseSlug)) baseSlug = "tac-gia";
            var uniqueSlug = baseSlug;
            var counter = 1;

            while (await _context.Authors.IgnoreQueryFilters().AnyAsync(a => a.Slug == uniqueSlug, cancellationToken))
            {
                counter++;
                uniqueSlug = $"{baseSlug}-{counter}";
            }

            var newAuthor = new Author
            {
                Name = cleanName,
                Slug = uniqueSlug,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _context.Authors.Add(newAuthor);
            await _context.SaveChangesAsync(cancellationToken);
            return newAuthor.Id;
        }

        if (authorId > 0)
        {
            var exists = await _context.Authors.AnyAsync(a => a.Id == authorId && !a.IsDeleted, cancellationToken);
            if (exists) return authorId;
        }

        throw new ApplicationException("Vui lòng chọn hoặc nhập tên tác giả hợp lệ.");
    }

    private static string GenerateSlug(string text)
    {
        return SlugHelper.GenerateSlug(text);
    }

    public async Task<ServiceResult<int>> CreateStoryAsync(CreateStoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var slugExists = await _context.Stories
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Slug == dto.Slug && !s.IsDeleted, cancellationToken);

        if (slugExists)
        {
            return ServiceResult<int>.Failure("Đường dẫn tĩnh (Slug) đã tồn tại trong hệ thống.");
        }

        var resolvedAuthorId = await ResolveAuthorIdAsync(dto.AuthorId, dto.AuthorName, cancellationToken);

        var story = new Story
        {
            Title = dto.Title.Trim(),
            Slug = dto.Slug.Trim().ToLowerInvariant(),
            AlternativeTitle = dto.AlternativeTitle?.Trim(),
            Description = dto.Description,
            CoverImageUrl = dto.CoverImageUrl,
            AuthorId = resolvedAuthorId,
            Status = dto.Status,
            PublishStatus = dto.PublishStatus,
            IsFeatured = dto.IsFeatured,
            CreatedBy = _currentUserService.UserId ?? "admin",
            PublishedAt = dto.PublishStatus == PublishStatus.Published ? DateTimeOffset.UtcNow : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        foreach (var catId in dto.CategoryIds.Distinct())
        {
            story.StoryCategories.Add(new StoryCategory { CategoryId = catId });
        }

        _context.Stories.Add(story);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<int>.Success(story.Id, "Đăng truyện mới thành công.");
    }

    public async Task<ServiceResult> UpdateStoryAsync(UpdateStoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .Include(s => s.StoryCategories)
            .FirstOrDefaultAsync(s => s.Id == dto.Id && !s.IsDeleted, cancellationToken);

        if (story == null)
        {
            return ServiceResult.Failure("Không tìm thấy truyện cần cập nhật.");
        }

        // Check slug uniqueness
        var slugConflict = await _context.Stories
            .IgnoreQueryFilters()
            .AnyAsync(s => s.Slug == dto.Slug && s.Id != dto.Id && !s.IsDeleted, cancellationToken);

        if (slugConflict)
        {
            return ServiceResult.Failure("Slug truyện này đã bị trùng với một truyện khác.");
        }

        // Optimistic Concurrency check
        _context.Entry(story).Property(s => s.RowVersion).OriginalValue = dto.RowVersion;

        var resolvedAuthorId = await ResolveAuthorIdAsync(dto.AuthorId, dto.AuthorName, cancellationToken);

        story.Title = dto.Title.Trim();
        story.Slug = dto.Slug.Trim().ToLowerInvariant();
        story.AlternativeTitle = dto.AlternativeTitle?.Trim();
        story.Description = dto.Description;
        story.CoverImageUrl = NormalizeCoverImageUrl(dto.CoverImageUrl);
        story.AuthorId = resolvedAuthorId;
        story.Status = dto.Status;
        story.PublishStatus = dto.PublishStatus;
        story.IsFeatured = dto.IsFeatured;
        story.UpdatedAt = DateTimeOffset.UtcNow;

        if (dto.PublishStatus == PublishStatus.Published && story.PublishedAt == null)
        {
            story.PublishedAt = DateTimeOffset.UtcNow;
        }

        // Sync Categories
        story.StoryCategories.Clear();
        foreach (var catId in dto.CategoryIds.Distinct())
        {
            story.StoryCategories.Add(new StoryCategory { StoryId = story.Id, CategoryId = catId });
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            return ServiceResult.Success("Cập nhật truyện thành công.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return ServiceResult.Failure("Dữ liệu truyện đã bị người khác thay đổi trước đó. Vui lòng tải lại trang và thử lại.");
        }
    }

    public async Task<ServiceResult> RemoveStoryAsync(RemoveStoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == dto.StoryId && !s.IsDeleted, cancellationToken);

        if (story == null) return ServiceResult.Failure("Không tìm thấy truyện cần gỡ.");

        story.Status = StoryStatus.Removed;
        story.RemovedAt = DateTimeOffset.UtcNow;
        story.RemovalReason = dto.RemovalReason.Trim();
        story.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã gỡ truyện khỏi hệ thống công khai và lưu lý do gỡ.");
    }

    public async Task<ServiceResult> RestoreStoryAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted, cancellationToken);

        if (story == null) return ServiceResult.Failure("Không tìm thấy truyện cần khôi phục.");

        story.Status = StoryStatus.Published;
        story.RemovedAt = null;
        story.RemovalReason = null;
        story.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Khôi phục truyện thành công.");
    }

    public async Task<ServiceResult> SoftDeleteStoryAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var story = await _context.Stories
            .IgnoreQueryFilters()
            .Include(s => s.Chapters)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (story == null) return ServiceResult.Failure("Không tìm thấy truyện.");

        var now = DateTimeOffset.UtcNow;
        story.IsDeleted = true;
        story.DeletedAt = now;
        story.UpdatedAt = now;

        foreach (var ch in story.Chapters)
        {
            ch.IsDeleted = true;
            ch.DeletedAt = now;
            ch.UpdatedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã chuyển truyện vào thùng rác. Truyện và toàn bộ chương sẽ tự động xóa vĩnh viễn sau 7 ngày.");
    }

    private static string? NormalizeCoverImageUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return url;
        if (url.StartsWith("//app/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return "/uploads/" + url["//app/uploads/".Length..];
        }
        if (url.StartsWith("/app/uploads/", StringComparison.OrdinalIgnoreCase))
        {
            return "/uploads/" + url["/app/uploads/".Length..];
        }
        return url;
    }
}
