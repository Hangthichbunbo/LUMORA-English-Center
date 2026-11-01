using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.DTOs.Chapter;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class ChapterManagementController : Controller
{
    private readonly IChapterService _chapterService;
    private readonly IStoryService _storyService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ChapterManagementController(
        IChapterService chapterService,
        IStoryService storyService,
        IFileStorageService fileStorageService,
        IWebHostEnvironment webHostEnvironment)
    {
        _chapterService = chapterService;
        _storyService = storyService;
        _fileStorageService = fileStorageService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int storyId, CancellationToken cancellationToken)
    {
        var story = await _storyService.GetStoryForEditAsync(storyId, cancellationToken);
        if (story == null) return NotFound();

        var chapters = await _chapterService.GetChaptersByStoryIdAsync(storyId, cancellationToken);
        ViewBag.StoryId = storyId;
        ViewBag.StoryTitle = story.Title;
        ViewBag.StorySlug = story.Slug;

        return View(chapters);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int storyId, CancellationToken cancellationToken)
    {
        var story = await _storyService.GetStoryForEditAsync(storyId, cancellationToken);
        if (story == null) return NotFound();

        var chapters = await _chapterService.GetChaptersByStoryIdAsync(storyId, cancellationToken);
        decimal maxNumber = chapters.Count > 0 ? chapters.Max(c => c.ChapterNumber) : 0;
        int nextNumber = (int)Math.Floor(maxNumber) + 1;

        ViewBag.StoryId = storyId;
        ViewBag.StoryTitle = story.Title;

        return View(new CreateChapterDto
        {
            StoryId = storyId,
            ChapterNumber = nextNumber,
            Title = $"Chương {nextNumber}: ",
            Slug = $"chuong-{nextNumber}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateChapterDto dto, CancellationToken cancellationToken)
    {
        var imageError = CheckChapterImages(dto.Content);
        if (!string.IsNullOrEmpty(imageError))
        {
            ModelState.AddModelError(nameof(dto.Content), imageError);
        }

        if (!ModelState.IsValid)
        {
            var story = await _storyService.GetStoryForEditAsync(dto.StoryId, cancellationToken);
            ViewBag.StoryId = dto.StoryId;
            ViewBag.StoryTitle = story?.Title;
            return View(dto);
        }

        var result = await _chapterService.CreateChapterAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            var story = await _storyService.GetStoryForEditAsync(dto.StoryId, cancellationToken);
            ViewBag.StoryId = dto.StoryId;
            ViewBag.StoryTitle = story?.Title;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Thêm chương mới thành công!";
        return RedirectToAction(nameof(Index), new { storyId = dto.StoryId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var chapter = await _chapterService.GetChapterForEditAsync(id, cancellationToken);
        if (chapter == null) return NotFound();

        var story = await _storyService.GetStoryForEditAsync(chapter.StoryId, cancellationToken);
        ViewBag.StoryId = chapter.StoryId;
        ViewBag.StoryTitle = story?.Title;

        return View(chapter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateChapterDto dto, CancellationToken cancellationToken)
    {
        var imageError = CheckChapterImages(dto.Content);
        if (!string.IsNullOrEmpty(imageError))
        {
            ModelState.AddModelError(nameof(dto.Content), imageError);
        }

        if (!ModelState.IsValid)
        {
            var story = await _storyService.GetStoryForEditAsync(dto.StoryId, cancellationToken);
            ViewBag.StoryId = dto.StoryId;
            ViewBag.StoryTitle = story?.Title;
            return View(dto);
        }

        var result = await _chapterService.UpdateChapterAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            var story = await _storyService.GetStoryForEditAsync(dto.StoryId, cancellationToken);
            ViewBag.StoryId = dto.StoryId;
            ViewBag.StoryTitle = story?.Title;
            return View(dto);
        }

        TempData["SuccessMessage"] = "Cập nhật chương thành công!";
        return RedirectToAction(nameof(Index), new { storyId = dto.StoryId });
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Không tìm thấy tệp ảnh tải lên." });
        }

        const long maxBytes = 1 * 1024 * 1024; // 1 MB
        if (file.Length > maxBytes)
        {
            return BadRequest(new { message = $"Dung lượng ảnh ({file.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa 1 MB. Vui lòng chọn ảnh nhỏ hơn." });
        }

        try
        {
            using var stream = file.OpenReadStream();
            var relativeUrl = await _fileStorageService.SaveFileAsync(stream, file.FileName, "chapters", cancellationToken);
            return Json(new { location = relativeUrl });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? CheckChapterImages(string? content)
    {
        if (string.IsNullOrWhiteSpace(content)) return null;

        const long maxBytes = 1 * 1024 * 1024; // 1 MB

        // 1. Check inline base64 images
        var base64Matches = Regex.Matches(content, @"<img\s+[^>]*?src=[""']data:image\/[^;]+;base64,([A-Za-z0-9+/=\s]+)[""']", RegexOptions.IgnoreCase);
        foreach (Match match in base64Matches)
        {
            var clean = match.Groups[1].Value.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");
            long approxBytes = (clean.Length * 3) / 4;
            if (approxBytes > maxBytes)
            {
                return $"Hình ảnh đính kèm trong nội dung chương ({approxBytes / 1024.0 / 1024.0:F2} MB) vượt quá dung lượng tối đa 1 MB. Vui lòng nén hoặc chọn ảnh nhỏ hơn.";
            }
        }

        // 2. Check local uploaded image files on disk referenced in content
        var imgMatches = Regex.Matches(content, @"<img\s+[^>]*?src=[""']([^""']+)[""']", RegexOptions.IgnoreCase);
        var webRoot = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");

        foreach (Match match in imgMatches)
        {
            var src = match.Groups[1].Value.Trim();
            if (src.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase) || src.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                var clean = src.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var fullPath = Path.Combine(webRoot, clean);
                if (System.IO.File.Exists(fullPath))
                {
                    var fileInfo = new FileInfo(fullPath);
                    if (fileInfo.Length > maxBytes)
                    {
                        return $"Hình ảnh '{Path.GetFileName(fullPath)}' ({fileInfo.Length / 1024.0 / 1024.0:F2} MB) trong nội dung chương vượt quá dung lượng tối đa 1 MB.";
                    }
                }
            }
        }

        return null;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int storyId, CancellationToken cancellationToken)
    {
        var result = await _chapterService.DeleteChapterAsync(id, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index), new { storyId = storyId });
    }
}
