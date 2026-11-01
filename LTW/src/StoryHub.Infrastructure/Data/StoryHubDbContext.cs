using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data.Interceptors;

namespace StoryHub.Infrastructure.Data;

public class StoryHubDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>, IAppDbContext
{
    private readonly AuditSaveChangesInterceptor? _auditInterceptor;

    public StoryHubDbContext(DbContextOptions<StoryHubDbContext> options) : base(options)
    {
    }

    public StoryHubDbContext(
        DbContextOptions<StoryHubDbContext> options,
        AuditSaveChangesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Author> Authors => Set<Author>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<StoryCategory> StoryCategories => Set<StoryCategory>();
    public DbSet<Rating> Ratings => Set<Rating>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentReport> CommentReports => Set<CommentReport>();
    public DbSet<FollowedStory> FollowedStories => Set<FollowedStory>();
    public DbSet<ReadingHistory> ReadingHistories => Set<ReadingHistory>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (_auditInterceptor != null)
        {
            optionsBuilder.AddInterceptors(_auditInterceptor);
        }
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all configurations defined in this assembly
        builder.ApplyConfigurationsFromAssembly(typeof(StoryHubDbContext).Assembly);
    }
}
