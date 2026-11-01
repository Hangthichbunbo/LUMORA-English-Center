using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Trash;

namespace StoryHub.Application.Interfaces;

public interface ITrashService
{
    Task<TrashSummaryDto> GetTrashSummaryAsync(CancellationToken cancellationToken = default);
    Task<PaginatedList<TrashStoryDto>> GetDeletedStoriesAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<PaginatedList<TrashChapterDto>> GetDeletedChaptersAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
    Task<ServiceResult> RestoreStoryAsync(int storyId, CancellationToken cancellationToken = default);
    Task<ServiceResult> RestoreChapterAsync(int chapterId, CancellationToken cancellationToken = default);
    Task<ServiceResult> PermanentlyDeleteStoryAsync(int storyId, CancellationToken cancellationToken = default);
    Task<ServiceResult> PermanentlyDeleteChapterAsync(int chapterId, CancellationToken cancellationToken = default);
    Task<int> CleanupExpiredItemsAsync(CancellationToken cancellationToken = default);
}

