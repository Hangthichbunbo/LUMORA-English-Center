using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Category;

namespace StoryHub.Application.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllCategoriesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CreateOrUpdateCategoryDto?> GetCategoryForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateCategoryAsync(CreateOrUpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateCategoryAsync(CreateOrUpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
