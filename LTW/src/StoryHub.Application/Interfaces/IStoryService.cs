using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Story;
using StoryHub.Domain.Enums;

namespace StoryHub.Application.Interfaces;

public interface IStoryService
{
    // Public queries
    Task<PaginatedList<StoryDto>> GetStoriesPaginatedAsync(StoryFilterDto filter, CancellationToken cancellationToken = default);
    Task<StoryDetailDto?> GetStoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<List<StoryDto>> GetFeaturedStoriesAsync(int count = 6, CancellationToken cancellationToken = default);
    Task<List<StoryDto>> GetLatestUpdatedStoriesAsync(int count = 12, CancellationToken cancellationToken = default);
    Task<List<StoryDto>> GetTopViewedStoriesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<StoryDto>> GetTopRatedStoriesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<StoryDto>> GetCompletedStoriesAsync(int count = 10, CancellationToken cancellationToken = default);

    // User interactions
    Task<ServiceResult<bool>> ToggleFollowStoryAsync(int storyId, CancellationToken cancellationToken = default);
    Task<PaginatedList<StoryDto>> GetFollowedStoriesAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ServiceResult> SaveReadingProgressAsync(int storyId, int chapterId, decimal progress, CancellationToken cancellationToken = default);
    Task<PaginatedList<StoryDto>> GetReadingHistoriesAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);

    // Admin operations (Strictly Admin Role Only!)
    Task<PaginatedList<StoryDto>> GetAdminStoriesPaginatedAsync(string? search, StoryStatus? status, int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<UpdateStoryDto?> GetStoryForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateStoryAsync(CreateStoryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateStoryAsync(UpdateStoryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> RemoveStoryAsync(RemoveStoryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> RestoreStoryAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult> SoftDeleteStoryAsync(int id, CancellationToken cancellationToken = default);
}
