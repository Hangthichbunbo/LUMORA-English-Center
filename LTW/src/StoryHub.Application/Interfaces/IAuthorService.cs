using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Author;

namespace StoryHub.Application.Interfaces;

public interface IAuthorService
{
    Task<List<AuthorDto>> GetAllAuthorsAsync(CancellationToken cancellationToken = default);
    Task<AuthorDto?> GetAuthorBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<CreateOrUpdateAuthorDto?> GetAuthorForEditAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateAuthorAsync(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateAuthorAsync(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteAuthorAsync(int id, CancellationToken cancellationToken = default);
}
