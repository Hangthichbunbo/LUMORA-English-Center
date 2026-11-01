using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Enums;
using StoryHub.Web.ViewModels;

namespace StoryHub.Web.Controllers;

public class HomeController : Controller
{
    private readonly IStoryService _storyService;
    private readonly ICategoryService _categoryService;

    public HomeController(IStoryService storyService, ICategoryService categoryService)
    {
        _storyService = storyService;
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = new HomeViewModel
        {
            FeaturedStories = await _storyService.GetFeaturedStoriesAsync(6, cancellationToken),
            LatestStories = await _storyService.GetLatestUpdatedStoriesAsync(12, cancellationToken),
            TopViewedStories = await _storyService.GetTopViewedStoriesAsync(10, cancellationToken),
            TopRatedStories = await _storyService.GetTopRatedStoriesAsync(10, cancellationToken),
            CompletedStories = await _storyService.GetCompletedStoriesAsync(6, cancellationToken),
            Categories = await _categoryService.GetAllCategoriesAsync(true, cancellationToken)
        };

        return View(model);
    }

    [HttpGet("the-loai/{slug}")]
    public IActionResult Category(string slug)
    {
        return RedirectToAction("Index", "Story", new { CategorySlug = slug });
    }

    [HttpGet("tac-gia/{slug}")]
    public IActionResult Author(string slug)
    {
        return RedirectToAction("Index", "Story", new { AuthorSlug = slug });
    }

    [HttpGet("chinh-sach-bao-mat")]
    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet("sitemap.xml")]
    [ResponseCache(Duration = 1800, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Sitemap([FromServices] IAppDbContext dbContext, CancellationToken cancellationToken)
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var stories = await dbContext.Stories
            .AsNoTracking()
            .Where(s => s.PublishStatus == PublishStatus.Published)
            .Select(s => new { s.Slug, s.UpdatedAt, s.CreatedAt })
            .ToListAsync(cancellationToken);

        var categories = await dbContext.Categories
            .AsNoTracking()
            .Select(c => new { c.Slug })
            .ToListAsync(cancellationToken);

        var sb = new StringBuilder();
        sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");

        // Trang chủ
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/</loc>");
        sb.AppendLine($"    <lastmod>{DateTime.UtcNow:yyyy-MM-dd}</lastmod>");
        sb.AppendLine("    <changefreq>hourly</changefreq>");
        sb.AppendLine("    <priority>1.0</priority>");
        sb.AppendLine("  </url>");

        // Danh sách truyện
        sb.AppendLine("  <url>");
        sb.AppendLine($"    <loc>{baseUrl}/truyen</loc>");
        sb.AppendLine("    <changefreq>daily</changefreq>");
        sb.AppendLine("    <priority>0.9</priority>");
        sb.AppendLine("  </url>");

        // Danh mục thể loại
        foreach (var cat in categories)
        {
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/the-loai/{cat.Slug}</loc>");
            sb.AppendLine("    <changefreq>daily</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        // Từng bộ truyện
        foreach (var story in stories)
        {
            var lastMod = (story.UpdatedAt ?? story.CreatedAt).ToString("yyyy-MM-dd");
            sb.AppendLine("  <url>");
            sb.AppendLine($"    <loc>{baseUrl}/truyen/{story.Slug}</loc>");
            sb.AppendLine($"    <lastmod>{lastMod}</lastmod>");
            sb.AppendLine("    <changefreq>daily</changefreq>");
            sb.AppendLine("    <priority>0.8</priority>");
            sb.AppendLine("  </url>");
        }

        sb.AppendLine("</urlset>");

        return Content(sb.ToString(), "application/xml", Encoding.UTF8);
    }

    [HttpPost("api/ads/click/{id:int}")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> TrackAdClick(int id, [FromServices] IAdvertisementService adService, CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest();
        await adService.TrackAdClickAsync(id, cancellationToken);
        return NoContent();
    }
}
