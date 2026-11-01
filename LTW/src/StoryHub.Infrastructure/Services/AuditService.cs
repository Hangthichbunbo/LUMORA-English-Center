using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Exceptions;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;

namespace StoryHub.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly StoryHubDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AuditService(StoryHubDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    private void EnsureAdminRole()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new ForbiddenAccessException("Chỉ Quản trị viên (Admin) mới có quyền truy cập Audit Log.");
        }
    }

    public async Task<PaginatedList<AuditLog>> GetAuditLogsPaginatedAsync(
        string? entityName = null,
        string? action = null,
        string? userId = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();

        var query = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(a => a.Action == action);
        }

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(a => a.UserId == userId);
        }

        var ordered = query.OrderByDescending(a => a.CreatedAt);

        return await PaginatedList<AuditLog>.CreateAsync(ordered, pageIndex, pageSize, cancellationToken);
    }

    public async Task<AuditLog?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        EnsureAdminRole();
        return await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }
}
