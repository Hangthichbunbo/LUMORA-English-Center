using Microsoft.EntityFrameworkCore;
using StoryHub.Domain.Entities;

namespace StoryHub.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Story> Stories { get; }
    DbSet<Chapter> Chapters { get; }
    DbSet<Author> Authors { get; }
    DbSet<Category> Categories { get; }
    DbSet<StoryCategory> StoryCategories { get; }
    DbSet<Rating> Ratings { get; }
    DbSet<Comment> Comments { get; }
    DbSet<CommentReport> CommentReports { get; }
    DbSet<FollowedStory> FollowedStories { get; }
    DbSet<ReadingHistory> ReadingHistories { get; }
    DbSet<Advertisement> Advertisements { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<ApplicationUser> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
