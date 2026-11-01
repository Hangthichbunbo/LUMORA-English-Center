using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.DTOs.Story;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Enums;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class StoryManagementController : Controller
{
    private readonly IStoryService _storyService;
    private readonly ICategoryService _categoryService;
    private readonly IAuthorService _authorService;
    private readonly IFileStorageService _fileStorageService;

    public StoryManagementController(
        IStoryService storyService,
        ICategoryService categoryService,
        IAuthorService authorService,
        IFileStorageService fileStorageService)
    {
        _storyService = storyService;
        _categoryService = categoryService;
        _authorService = authorService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search,
        StoryStatus? status,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        var stories = await _storyService.GetAdminStoriesPaginatedAsync(search, status, pageIndex, 15, cancellationToken);
        ViewData["Search"] = search;
        ViewData["Status"] = status;

        return View(stories);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);

        return View(new CreateStoryDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateStoryDto dto,
        IFormFile? coverImage,
        CancellationToken cancellationToken)
    {
        if (coverImage != null && coverImage.Length > 0)
        {
            if (coverImage.Length > 1 * 1024 * 1024)
            {
                ModelState.AddModelError("coverImage", $"Dung lượng ảnh bìa ({coverImage.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa 1 MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
            else
            {
                try
                {
                    using var stream = coverImage.OpenReadStream();
                    dto.CoverImageUrl = await _fileStorageService.SaveFileAsync(stream, coverImage.FileName, "covers", cancellationToken);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi tải ảnh bìa: " + ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
            return View(dto);
        }

        var result = await _storyService.CreateStoryAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
            return View(dto);
        }

        TempData["SuccessMessage"] = "Đăng truyện mới thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var story = await _storyService.GetStoryForEditAsync(id, cancellationToken);
        if (story == null) return NotFound();

        ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);

        return View(story);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        UpdateStoryDto dto,
        IFormFile? coverImage,
        CancellationToken cancellationToken)
    {
        string? oldCoverUrl = null;
        string? newCoverUrl = null;

        if (coverImage != null && coverImage.Length > 0)
        {
            if (coverImage.Length > 1 * 1024 * 1024)
            {
                ModelState.AddModelError("coverImage", $"Dung lượng ảnh bìa ({coverImage.Length / 1024.0 / 1024.0:F2} MB) vượt quá giới hạn tối đa 1 MB. Vui lòng chọn ảnh nhỏ hơn.");
            }
            else
            {
                try
                {
                    var existingStory = await _storyService.GetStoryForEditAsync(dto.Id, cancellationToken);
                    if (existingStory != null && !string.IsNullOrWhiteSpace(existingStory.CoverImageUrl))
                    {
                        oldCoverUrl = existingStory.CoverImageUrl;
                    }

                    using var stream = coverImage.OpenReadStream();
                    newCoverUrl = await _fileStorageService.SaveFileAsync(stream, coverImage.FileName, "covers", cancellationToken);
                    dto.CoverImageUrl = newCoverUrl;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Lỗi cập nhật ảnh bìa: " + ex.Message);
                }
            }
        }

        if (!ModelState.IsValid)
        {
            if (!string.IsNullOrWhiteSpace(newCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(newCoverUrl, cancellationToken);
            }
            ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
            return View(dto);
        }

        var result = await _storyService.UpdateStoryAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(newCoverUrl))
            {
                await _fileStorageService.DeleteFileAsync(newCoverUrl, cancellationToken);
            }
            ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? result.Message);
            ViewBag.Authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
            ViewBag.Categories = await _categoryService.GetAllCategoriesAsync(false, cancellationToken);
            return View(dto);
        }

        // Deletion of old cover from server upon successful update
        if (!string.IsNullOrWhiteSpace(newCoverUrl) && !string.IsNullOrWhiteSpace(oldCoverUrl) && oldCoverUrl != newCoverUrl)
        {
            await _fileStorageService.DeleteFileAsync(oldCoverUrl, cancellationToken);
        }

        TempData["SuccessMessage"] = "Cập nhật truyện thành công!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int storyId, string removalReason, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(removalReason) || removalReason.Trim().Length < 5)
        {
            TempData["ErrorMessage"] = "Lý do gỡ truyện phải có ít nhất 5 ký tự.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _storyService.RemoveStoryAsync(new RemoveStoryDto
        {
            StoryId = storyId,
            RemovalReason = removalReason
        }, cancellationToken);

        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _storyService.RestoreStoryAsync(id, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _storyService.SoftDeleteStoryAsync(id, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
