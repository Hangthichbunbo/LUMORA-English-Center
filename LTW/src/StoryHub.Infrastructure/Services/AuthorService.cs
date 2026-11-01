using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Helpers;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Author;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class AuthorService : IAuthorService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuthorService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AuthorDto>> GetAllAuthorsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .AsNoTracking()
            .OrderBy(a => a.Name)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                Name = a.Name,
                Slug = a.Slug,
                Biography = a.Biography,
                StoryCount = a.Stories.Count(s => !s.IsDeleted)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AuthorDto?> GetAuthorBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Authors
            .AsNoTracking()
            .Where(a => a.Slug == slug)
            .Select(a => new AuthorDto
            {
                Id = a.Id,
                Name = a.Name,
                Slug = a.Slug,
                Biography = a.Biography,
                StoryCount = a.Stories.Count(s => !s.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền quản lý tác giả.");
        }
    }

    public async Task<CreateOrUpdateAuthorDto?> GetAuthorForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var author = await _context.Authors.FindAsync([id], cancellationToken);
        if (author == null) return null;

        return new CreateOrUpdateAuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            Slug = author.Slug,
            Biography = author.Biography
        };
    }

    public async Task<ServiceResult<int>> CreateAuthorAsync(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            dto.Slug = SlugHelper.GenerateSlug(dto.Name);
        }

        var exists = await _context.Authors.AnyAsync(a => a.Slug == dto.Slug, cancellationToken);
        if (exists) return ServiceResult<int>.Failure("Slug tác giả đã tồn tại.");

        var author = new Author
        {
            Name = dto.Name.Trim(),
            Slug = dto.Slug.Trim().ToLowerInvariant(),
            Biography = dto.Biography?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<int>.Success(author.Id, "Tạo tác giả mới thành công.");
    }

    public async Task<ServiceResult> UpdateAuthorAsync(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            dto.Slug = SlugHelper.GenerateSlug(dto.Name);
        }

        var author = await _context.Authors.FindAsync([dto.Id], cancellationToken);
        if (author == null) return ServiceResult.Failure("Không tìm thấy tác giả.");

        var slugConflict = await _context.Authors.AnyAsync(a => a.Slug == dto.Slug && a.Id != dto.Id, cancellationToken);
        if (slugConflict) return ServiceResult.Failure("Slug tác giả đã bị trùng.");

        author.Name = dto.Name.Trim();
        author.Slug = dto.Slug.Trim().ToLowerInvariant();
        author.Biography = dto.Biography?.Trim();
        author.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Cập nhật tác giả thành công.");
    }

    public async Task<ServiceResult> DeleteAuthorAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var author = await _context.Authors.FindAsync([id], cancellationToken);
        if (author == null) return ServiceResult.Failure("Không tìm thấy tác giả.");

        author.IsDeleted = true;
        author.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã xóa tác giả.");
    }
}
