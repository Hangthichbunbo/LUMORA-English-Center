using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Helpers;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Category;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CategoryService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<CategoryDto>> GetAllCategoriesAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories.AsNoTracking();
        if (activeOnly) query = query.Where(c => c.IsActive);

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                StoryCount = c.StoryCategories.Count(sc => !sc.Story.IsDeleted)
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryDto?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .Where(c => c.Slug == slug)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                IsActive = c.IsActive,
                StoryCount = c.StoryCategories.Count(sc => !sc.Story.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền quản lý thể loại.");
        }
    }

    public async Task<CreateOrUpdateCategoryDto?> GetCategoryForEditAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var cat = await _context.Categories.FindAsync([id], cancellationToken);
        if (cat == null) return null;

        return new CreateOrUpdateCategoryDto
        {
            Id = cat.Id,
            Name = cat.Name,
            Slug = cat.Slug,
            Description = cat.Description,
            IsActive = cat.IsActive
        };
    }

    public async Task<ServiceResult<int>> CreateCategoryAsync(CreateOrUpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            dto.Slug = SlugHelper.GenerateSlug(dto.Name);
        }

        var exists = await _context.Categories.AnyAsync(c => c.Slug == dto.Slug, cancellationToken);
        if (exists) return ServiceResult<int>.Failure("Slug thể loại đã tồn tại.");

        var cat = new Category
        {
            Name = dto.Name.Trim(),
            Slug = dto.Slug.Trim().ToLowerInvariant(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.Categories.Add(cat);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<int>.Success(cat.Id, "Tạo thể loại thành công.");
    }

    public async Task<ServiceResult> UpdateCategoryAsync(CreateOrUpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        if (string.IsNullOrWhiteSpace(dto.Slug))
        {
            dto.Slug = SlugHelper.GenerateSlug(dto.Name);
        }

        var cat = await _context.Categories.FindAsync([dto.Id], cancellationToken);
        if (cat == null) return ServiceResult.Failure("Không tìm thấy thể loại.");

        var slugConflict = await _context.Categories.AnyAsync(c => c.Slug == dto.Slug && c.Id != dto.Id, cancellationToken);
        if (slugConflict) return ServiceResult.Failure("Slug thể loại đã bị trùng.");

        cat.Name = dto.Name.Trim();
        cat.Slug = dto.Slug.Trim().ToLowerInvariant();
        cat.Description = dto.Description?.Trim();
        cat.IsActive = dto.IsActive;
        cat.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Cập nhật thể loại thành công.");
    }

    public async Task<ServiceResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var cat = await _context.Categories.FindAsync([id], cancellationToken);
        if (cat == null) return ServiceResult.Failure("Không tìm thấy thể loại.");

        _context.Categories.Remove(cat);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult.Success("Đã xóa thể loại.");
    }
}
