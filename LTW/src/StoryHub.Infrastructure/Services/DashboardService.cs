using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.DTOs.Dashboard;
using StoryHub.Application.DTOs.Story;
using StoryHub.Domain.Enums;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DashboardService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsInRole("Admin") && !_currentUserService.IsInRole("Moderator"))
        {
            throw new ForbiddenAccessException("Bạn không có quyền truy cập Dashboard quản trị.");
        }

        var now = DateTimeOffset.UtcNow;

        var totalStories = await _context.Stories.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted, cancellationToken);
        var totalChapters = await _context.Chapters.IgnoreQueryFilters().CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalComments = await _context.Comments.CountAsync(c => !c.IsDeleted, cancellationToken);
        var totalViews = await _context.Stories.IgnoreQueryFilters().Where(s => !s.IsDeleted).SumAsync(s => s.ViewCount, cancellationToken);

        var publishedCount = await _context.Stories.CountAsync(s => s.Status == StoryStatus.Published, cancellationToken);
        var hiddenOrRemoved = await _context.Stories.IgnoreQueryFilters().CountAsync(s => !s.IsDeleted && (s.Status == StoryStatus.Hidden || s.Status == StoryStatus.Removed), cancellationToken);

        var pendingComments = await _context.Comments.CountAsync(c => c.Status == CommentStatus.Pending && !c.IsDeleted, cancellationToken);
        var pendingReports = await _context.CommentReports.CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken);
        var activeAds = await _context.Advertisements.CountAsync(a => a.IsEnabled && a.StartAt <= now && a.EndAt >= now, cancellationToken);

        var recentStories = await _context.Stories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(s => !s.IsDeleted)
            .OrderByDescending(s => s.UpdatedAt ?? s.CreatedAt)
            .Take(5)
            .Select(s => new StoryDto
            {
                Id = s.Id,
                Title = s.Title,
                Slug = s.Slug,
                CoverImageUrl = s.CoverImageUrl,
                AuthorName = s.Author.Name,
                Status = s.Status,
                ViewCount = s.ViewCount,
                UpdatedAt = s.UpdatedAt ?? s.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Generate synthetic view statistics for the last 7 days chart
        var dailyStats = new List<DailyViewStatDto>();
        for (int i = 6; i >= 0; i--)
        {
            var date = DateTime.UtcNow.AddDays(-i).ToString("dd/MM");
            dailyStats.Add(new DailyViewStatDto
            {
                Date = date,
                Views = (totalViews / 30) + (i * 120) + (i % 2 == 0 ? 350 : 150)
            });
        }

        return new AdminDashboardDto
        {
            TotalStories = totalStories,
            TotalChapters = totalChapters,
            TotalUsers = totalUsers,
            TotalComments = totalComments,
            TotalViews = totalViews,
            PublishedStoriesCount = publishedCount,
            HiddenOrRemovedStoriesCount = hiddenOrRemoved,
            PendingCommentsCount = pendingComments,
            PendingReportsCount = pendingReports,
            ActiveAdsCount = activeAds,
            RecentStories = recentStories,
            DailyViewStats = dailyStats
        };
    }
}
