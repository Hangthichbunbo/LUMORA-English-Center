using StoryHub.Application.Common.Models;
using StoryHub.Domain.Entities;

namespace StoryHub.Application.Interfaces;

public interface IAuditService
{
    Task<PaginatedList<AuditLog>> GetAuditLogsPaginatedAsync(
        string? entityName = null,
        string? action = null,
        string? userId = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<AuditLog?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default);
}
