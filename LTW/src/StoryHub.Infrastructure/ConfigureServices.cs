using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Entities;
using StoryHub.Infrastructure.Data;
using StoryHub.Infrastructure.Data.Interceptors;
using StoryHub.Infrastructure.Services;

namespace StoryHub.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<AuditSaveChangesInterceptor>();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' không được tìm thấy.");

        services.AddDbContext<StoryHubDbContext>((sp, options) =>
        {
            var interceptor = sp.GetRequiredService<AuditSaveChangesInterceptor>();
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(StoryHubDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
            options.AddInterceptors(interceptor);
        });

        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<StoryHubDbContext>());

        // ASP.NET Core Identity Configuration
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password policy
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout policy (Brute-force protection)
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User policy
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<StoryHubDbContext>()
        .AddDefaultTokenProviders()
        .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>();

        // Security & Utility Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddSingleton<IHtmlSanitizationService, GanssHtmlSanitizationService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        // High-performance View Count buffer & worker
        services.AddSingleton<IViewCountBufferService, ViewCountBufferService>();
        services.AddHostedService<ViewCountBackgroundWorker>();
        services.AddHostedService<TrashCleanupBackgroundService>();

        // Domain & Application Services
        services.AddScoped<IStoryService, StoryService>();
        services.AddScoped<IChapterService, ChapterService>();
        services.AddScoped<ITrashService, TrashService>();
        services.AddScoped<IRatingService, RatingService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAdvertisementService, AdvertisementService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IDashboardService, DashboardService>();

        return services;
    }
}
