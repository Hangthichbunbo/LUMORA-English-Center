using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Chapter;

namespace StoryHub.Application.Interfaces;

public interface IChapterService
{
    Task<List<ChapterDto>> GetChaptersByStoryIdAsync(int storyId, CancellationToken cancellationToken = default);
    Task<ChapterDetailDto?> GetChapterForReadingAsync(string storySlug, decimal chapterNumber, string chapterSlug, CancellationToken cancellationToken = default);
    Task<UpdateChapterDto?> GetChapterForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateChapterAsync(CreateChapterDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateChapterAsync(UpdateChapterDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteChapterAsync(int id, CancellationToken cancellationToken = default);
}
