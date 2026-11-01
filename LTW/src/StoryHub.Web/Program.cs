using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using StoryHub.Application;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Options;
using StoryHub.Infrastructure;
using StoryHub.Infrastructure.Data;
using StoryHub.Infrastructure.Data.Seed;
using StoryHub.Web.Middleware;
using StoryHub.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Kestrel Server Hardening: Disable Server Header disclosure and enforce request limits
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.AddServerHeader = false;
    serverOptions.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
    serverOptions.Limits.MaxRequestHeaderCount = 100;
    serverOptions.Limits.MaxRequestHeadersTotalSize = 32768; // 32KB
});

// Options configuration
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(SecuritySettings.SectionName));
builder.Services.Configure<StorageSettings>(builder.Configuration.GetSection(StorageSettings.SectionName));
builder.Services.Configure<ViewCountSettings>(builder.Configuration.GetSection(ViewCountSettings.SectionName));
builder.Services.Configure<SeedSettings>(builder.Configuration.GetSection(SeedSettings.SectionName));
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection(EmailSettings.SectionName));

// Context & Current User & Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ICaptchaService, CaptchaService>();

// Session Support (For Captcha & temporary state)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(15);
    options.Cookie.Name = ".TruyenVietSub.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// Add Application services (Validators, etc.)
builder.Services.AddApplicationServices();

// Add Infrastructure services (EF Core, Identity, Buffers, Repositories)
builder.Services.AddInfrastructureServices(builder.Configuration);

// HSTS configuration
builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365);
});

// Cookie Security & Session Hardening
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".TruyenVietSub.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.LoginPath = "/dang-nhap";
    options.AccessDeniedPath = "/Error/403";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);

    options.Events.OnRedirectToLogin = context =>
    {
        var isAjax = context.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || context.Request.Headers.Accept.ToString().Contains("application/json")
            || context.Request.ContentType?.Contains("application/json") == true;

        if (isAjax)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json; charset=utf-8";
            return context.Response.WriteAsync("{\"success\":false,\"message\":\"Vui lòng đăng nhập để thực hiện thao tác này.\"}");
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        var isAjax = context.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || context.Request.Headers.Accept.ToString().Contains("application/json")
            || context.Request.ContentType?.Contains("application/json") == true;

        if (isAjax)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json; charset=utf-8";
            return context.Response.WriteAsync("{\"success\":false,\"message\":\"Bạn không có quyền thực hiện thao tác này.\"}");
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

// Antiforgery Cookie & Global AutoValidate
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = ".TruyenVietSub.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.HeaderName = "X-XSRF-TOKEN";
});

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Rate Limiting Policies (.NET 10 built-in)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        var isAjax = context.HttpContext.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || context.HttpContext.Request.Headers.Accept.ToString().Contains("application/json")
            || context.HttpContext.Request.ContentType?.Contains("application/json") == true;

        if (isAjax)
        {
            context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            await context.HttpContext.Response.WriteAsync(
                "{\"success\":false,\"message\":\"Bạn đang gửi thao tác quá nhanh. Vui lòng thử lại sau giây lát.\"}",
                token);
        }
        else
        {
            context.HttpContext.Response.Redirect("/Error/429");
        }
    };

    var authPermitLimit = builder.Environment.IsEnvironment("Testing") ? 1000 : 5;
    var captchaPermitLimit = builder.Environment.IsEnvironment("Testing") ? 1000 : 30;

    // Auth Policy: Limit login/register attempts per IP to prevent brute-force attacks
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = authPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Comment Policy: Limit comment submissions per User (or IP if unauthenticated)
    options.AddPolicy("comment", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_user",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Rating Policy: Limit rating attempts per User
    options.AddPolicy("rating", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_user",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    // Captcha Policy: Limit captcha requests to prevent resource exhaustion attacks
    options.AddPolicy("captcha", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown_ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = captchaPermitLimit,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));
});

var app = builder.Build();

// 1. Global Exception Handling (RFC 7807 ProblemDetails for API/AJAX & graceful View for HTML)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// 2. Modern Security Headers (CSP, nosniff, DENY frame-ancestors, Permissions-Policy)
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

var staticExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".css", ".js", ".map", ".json",
    ".png", ".jpg", ".jpeg", ".gif", ".webp", ".svg", ".ico",
    ".woff", ".woff2", ".ttf", ".eot", ".otf",
    ".mp3", ".mp4", ".webm", ".pdf", ".txt", ".xml", ".webmanifest"
};

app.UseWhen(
    context => !staticExtensions.Contains(Path.GetExtension(context.Request.Path.Value ?? string.Empty)) &&
               !context.Request.Path.StartsWithSegments("/api"),
    builder => builder.UseStatusCodePagesWithReExecute("/Error/{0}")
);

app.UseHttpsRedirection();

// Enhanced Static Files Provider for Production/Host Environments (ensures proper MIME types for Favicons and Manifests)
var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".webmanifest"] = "application/manifest+json";
contentTypeProvider.Mappings[".ico"] = "image/x-icon";
contentTypeProvider.Mappings[".svg"] = "image/svg+xml";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

// Support external/persistent Docker volumes for uploads (e.g. /app/uploads in Coolify)
var uploadFolderConfig = builder.Configuration.GetValue<string>("StorageSettings:UploadFolder") ?? "uploads";
var extraUploadPaths = new List<(string PhysPath, string ReqPath)>();

if (Path.IsPathRooted(uploadFolderConfig))
{
    extraUploadPaths.Add((uploadFolderConfig, "/uploads"));
    extraUploadPaths.Add((uploadFolderConfig, "/app/uploads"));
}
else if (Directory.Exists("/app/uploads"))
{
    extraUploadPaths.Add(("/app/uploads", "/uploads"));
    extraUploadPaths.Add(("/app/uploads", "/app/uploads"));
}

foreach (var item in extraUploadPaths)
{
    if (!Directory.Exists(item.PhysPath))
    {
        Directory.CreateDirectory(item.PhysPath);
    }
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(item.PhysPath),
        RequestPath = item.ReqPath,
        ContentTypeProvider = contentTypeProvider
    });
}

app.UseRouting();

// 3. Rate Limiter Middleware
app.UseRateLimiter();

// 4. Session Middleware
app.UseSession();

// 5. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto-run Migrations and Seed Data safely
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<StoryHubDbContext>();
        if (dbContext.Database.IsRelational())
        {
            await dbContext.Database.MigrateAsync();
        }
        await StoryHubDbSeeder.SeedAsync(services);
        logger.LogInformation("Database migration and seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Đã xảy ra lỗi khi khởi tạo hoặc cập nhật cơ sở dữ liệu TruyenVietSub.");
    }
}

app.Run();


public partial class Program { }
