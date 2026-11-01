using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Rating;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class RatingService : IRatingService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RatingService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<RatingSummaryDto>> SubmitRatingAsync(SubmitRatingDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.IsAuthenticated || string.IsNullOrEmpty(_currentUserService.UserId))
        {
            throw new ForbiddenAccessException("Vui lòng đăng nhập để đánh giá truyện.");
        }

        if (dto.Score < 1 || dto.Score > 5)
        {
            return ServiceResult<RatingSummaryDto>.Failure("Điểm đánh giá phải từ 1 đến 5 sao.");
        }

        var storyExists = await _context.Stories.AnyAsync(s => s.Id == dto.StoryId, cancellationToken);
        if (!storyExists)
        {
            return ServiceResult<RatingSummaryDto>.Failure("Không tìm thấy truyện cần đánh giá.");
        }

        var userId = _currentUserService.UserId;
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.StoryId == dto.StoryId && r.UserId == userId, cancellationToken);

        if (existingRating == null)
        {
            _context.Ratings.Add(new Rating
            {
                StoryId = dto.StoryId,
                UserId = userId,
                Score = dto.Score,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        else
        {
            existingRating.Score = dto.Score;
            existingRating.UpdatedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Recalculate accurately on the server
        await RecalculateStoryRatingAsync(dto.StoryId, cancellationToken);

        var summary = await GetStoryRatingSummaryAsync(dto.StoryId, cancellationToken);
        return ServiceResult<RatingSummaryDto>.Success(summary, "Gửi đánh giá truyện thành công.");
    }

    public async Task<RatingSummaryDto> GetStoryRatingSummaryAsync(int storyId, CancellationToken cancellationToken = default)
    {
        var story = await _context.Stories
            .AsNoTracking()
            .Where(s => s.Id == storyId)
            .Select(s => new { s.AverageRating, s.RatingCount })
            .FirstOrDefaultAsync(cancellationToken);

        int? currentRating = null;
        if (_currentUserService.IsAuthenticated && !string.IsNullOrEmpty(_currentUserService.UserId))
        {
            currentRating = await _context.Ratings
                .Where(r => r.StoryId == storyId && r.UserId == _currentUserService.UserId)
                .Select(r => (int?)r.Score)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new RatingSummaryDto
        {
            StoryId = storyId,
            AverageRating = story?.AverageRating ?? 0,
            RatingCount = story?.RatingCount ?? 0,
            CurrentUserRating = currentRating
        };
    }

    public async Task<ServiceResult> RecalculateStoryRatingAsync(int storyId, CancellationToken cancellationToken = default)
    {
        var ratings = await _context.Ratings
            .Where(r => r.StoryId == storyId)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        var story = await _context.Stories.FindAsync([storyId], cancellationToken);
        if (story == null) return ServiceResult.Failure("Không tìm thấy truyện.");

        if (ratings.Count == 0)
        {
            story.AverageRating = 0;
            story.RatingCount = 0;
        }
        else
        {
            story.AverageRating = Math.Round((decimal)ratings.Average(), 2);
            story.RatingCount = ratings.Count;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success();
    }
}
