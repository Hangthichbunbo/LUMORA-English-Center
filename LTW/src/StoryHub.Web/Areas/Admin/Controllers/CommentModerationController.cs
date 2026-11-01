using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Interfaces;
using StoryHub.Domain.Enums;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin,Moderator")]
public class CommentModerationController : Controller
{
    private readonly ICommentService _commentService;

    public CommentModerationController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CommentStatus? status, int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var comments = await _commentService.GetAdminCommentsPaginatedAsync(status, pageIndex, 20, cancellationToken);
        ViewData["Status"] = status;
        return View(comments);
    }

    [HttpGet]
    public async Task<IActionResult> Reports(ReportStatus? status, int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var reports = await _commentService.GetCommentReportsPaginatedAsync(status, pageIndex, 20, cancellationToken);
        ViewData["Status"] = status;
        return View(reports);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Moderate(int commentId, CommentStatus status, CancellationToken cancellationToken)
    {
        var result = await _commentService.ModerateCommentAsync(commentId, status, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReviewReport(int reportId, ReportStatus status, CancellationToken cancellationToken)
    {
        var result = await _commentService.ReviewReportAsync(reportId, status, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Reports));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockUser(string userId, int days, CancellationToken cancellationToken)
    {
        var result = await _commentService.BlockUserCommentAsync(userId, days, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Reports));
    }
}
