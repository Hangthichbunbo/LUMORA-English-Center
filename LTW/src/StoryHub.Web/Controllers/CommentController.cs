using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoryHub.Application.DTOs.Comment;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Controllers;

public class CommentController : Controller
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(int storyId, int? chapterId, int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        var comments = await _commentService.GetStoryCommentsAsync(storyId, chapterId, pageIndex, 10, cancellationToken);
        return Json(new { success = true, data = comments });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comment")]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
            return BadRequest(new { success = false, message = string.Join(" ", errors) });
        }

        var result = await _commentService.CreateCommentAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = result.Errors.FirstOrDefault() ?? result.Message });
        }

        return Json(new { success = true, data = result.Data, message = result.Message });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comment")]
    public async Task<IActionResult> Update([FromBody] UpdateCommentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _commentService.UpdateCommentAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = result.Errors.FirstOrDefault() ?? result.Message });
        }

        return Json(new { success = true, message = result.Message });
    }

    public record DeleteCommentRequest(int CommentId);

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comment")]
    public async Task<IActionResult> Delete([FromBody] DeleteCommentRequest request, CancellationToken cancellationToken)
    {
        if (request == null || request.CommentId <= 0)
        {
            return BadRequest(new { success = false, message = "Id bình luận không hợp lệ." });
        }

        var result = await _commentService.DeleteCommentAsync(request.CommentId, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = result.Errors.FirstOrDefault() ?? result.Message });
        }

        return Json(new { success = true, message = result.Message });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comment")]
    public async Task<IActionResult> Report([FromBody] ReportCommentDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Vui lòng chọn lý do báo cáo." });
        }

        var result = await _commentService.ReportCommentAsync(dto, cancellationToken);
        return Json(new { success = result.Succeeded, message = result.Message });
    }
}
