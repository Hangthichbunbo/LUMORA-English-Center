using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Domain.Common;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Interceptors;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private static readonly HashSet<string> SensitiveProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash",
        "SecurityStamp",
        "ConcurrencyStamp",
        "RowVersion",
        "Token"
    };

    public AuditSaveChangesInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return base.SavingChangesAsync(eventData, result, cancellationToken);

        UpdateAuditableEntities(context);
        GenerateAuditEntries(context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditableEntities(DbContext context)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in context.ChangeTracker.Entries<ISoftDeletable>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                {
                    entry.Entity.DeletedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                var isDeletedProp = entry.Property(e => e.IsDeleted);
                if (isDeletedProp.IsModified)
                {
                    if (entry.Entity.IsDeleted && entry.Entity.DeletedAt == null)
                    {
                        entry.Entity.DeletedAt = now;
                    }
                    else if (!entry.Entity.IsDeleted)
                    {
                        entry.Entity.DeletedAt = null;
                    }
                }
            }
        }
    }

    private void GenerateAuditEntries(DbContext context)
    {
        var auditLogs = new List<AuditLog>();
        var now = DateTimeOffset.UtcNow;

        var trackedEntries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog &&
                       (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in trackedEntries)
        {
            var entityName = entry.Entity.GetType().Name;

            // Only audit important domain models
            if (!ShouldAudit(entityName)) continue;

            var entityId = GetPrimaryKeyValue(entry);
            var action = entry.State.ToString();

            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                if (prop.IsTemporary || SensitiveProperties.Contains(prop.Metadata.Name))
                {
                    continue;
                }

                var propName = prop.Metadata.Name;

                switch (entry.State)
                {
                    case EntityState.Added:
                        newValues[propName] = prop.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        oldValues[propName] = prop.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (prop.IsModified)
                        {
                            oldValues[propName] = prop.OriginalValue;
                            newValues[propName] = prop.CurrentValue;
                        }
                        break;
                }
            }

            auditLogs.Add(new AuditLog
            {
                UserId = _currentUserService.UserId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                IpAddress = _currentUserService.IpAddress,
                UserAgent = _currentUserService.UserAgent,
                CorrelationId = _currentUserService.CorrelationId,
                CreatedAt = now
            });
        }

        if (auditLogs.Count > 0)
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private static bool ShouldAudit(string entityName)
    {
        return entityName is nameof(Story)
            or nameof(Chapter)
            or nameof(Advertisement)
            or nameof(Category)
            or nameof(Author)
            or nameof(Comment)
            or nameof(ApplicationUser);
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperty = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return keyProperty?.CurrentValue?.ToString() ?? "0";
    }
}
