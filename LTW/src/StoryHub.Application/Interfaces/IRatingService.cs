using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Rating;

namespace StoryHub.Application.Interfaces;

public interface IRatingService
{
    Task<ServiceResult<RatingSummaryDto>> SubmitRatingAsync(SubmitRatingDto dto, CancellationToken cancellationToken = default);
    Task<RatingSummaryDto> GetStoryRatingSummaryAsync(int storyId, CancellationToken cancellationToken = default);
    Task<ServiceResult> RecalculateStoryRatingAsync(int storyId, CancellationToken cancellationToken = default);
}
