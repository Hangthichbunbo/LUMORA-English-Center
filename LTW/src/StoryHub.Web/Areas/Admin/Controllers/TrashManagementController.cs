using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.DTOs.Trash;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TrashManagementController : Controller
{
    private readonly ITrashService _trashService;

    public TrashManagementController(ITrashService trashService)
    {
        _trashService = trashService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string tab = "stories",
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        var summary = await _trashService.GetTrashSummaryAsync(cancellationToken);
        ViewBag.Summary = summary;
        ViewBag.ActiveTab = tab.ToLowerInvariant() == "chapters" ? "chapters" : "stories";

        if (ViewBag.ActiveTab == "chapters")
        {
            var chapters = await _trashService.GetDeletedChaptersAsync(pageIndex, 15, cancellationToken);
            return View("Index", chapters);
        }
        else
        {
            var stories = await _trashService.GetDeletedStoriesAsync(pageIndex, 15, cancellationToken);
            return View("Index", stories);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreStory(int id, CancellationToken cancellationToken)
    {
        var result = await _trashService.RestoreStoryAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "stories" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestoreChapter(int id, CancellationToken cancellationToken)
    {
        var result = await _trashService.RestoreChapterAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "chapters" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermanentDeleteStory(int id, CancellationToken cancellationToken)
    {
        var result = await _trashService.PermanentlyDeleteStoryAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "stories" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PermanentDeleteChapter(int id, CancellationToken cancellationToken)
    {
        var result = await _trashService.PermanentlyDeleteChapterAsync(id, cancellationToken);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = result.Message;
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return RedirectToAction(nameof(Index), new { tab = "chapters" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CleanupExpired(CancellationToken cancellationToken)
    {
        var count = await _trashService.CleanupExpiredItemsAsync(cancellationToken);
        TempData["SuccessMessage"] = $"Đã dọn dẹp vĩnh viễn {count} mục hết hạn quá 7 ngày trong thùng rác.";
        return RedirectToAction(nameof(Index));
    }
}

