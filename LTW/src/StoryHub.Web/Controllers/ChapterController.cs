using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Interfaces;
using StoryHub.Web.ViewModels;

namespace StoryHub.Web.Controllers;

public class ChapterController : Controller
{
    private readonly IChapterService _chapterService;
    private readonly IStoryService _storyService;
    private readonly ICommentService _commentService;

    public ChapterController(
        IChapterService chapterService,
        IStoryService storyService,
        ICommentService commentService)
    {
        _chapterService = chapterService;
        _storyService = storyService;
        _commentService = commentService;
    }

    [HttpGet("truyen/{storySlug}/chuong-{chapterNumber:decimal}/{chapterSlug?}", Order = 1)]
    [HttpGet("truyen/{storySlug}/chuong-{chapterRef}", Order = 2)]
    [HttpGet("truyen/{storySlug}/chuong/{chapterRef}", Order = 3)]
    public async Task<IActionResult> Read(
        string storySlug,
        string? chapterRef = null,
        decimal? chapterNumber = null,
        string? chapterSlug = null,
        CancellationToken cancellationToken = default)
    {
        decimal resolvedChapterNumber = 1;
        string resolvedChapterSlug = chapterSlug ?? string.Empty;

        if (chapterNumber.HasValue)
        {
            resolvedChapterNumber = chapterNumber.Value;
        }
        else if (!string.IsNullOrWhiteSpace(chapterRef))
        {
            var match = System.Text.RegularExpressions.Regex.Match(chapterRef, @"^(\d+(\.\d+)?)");
            if (match.Success && decimal.TryParse(match.Value, System.Globalization.CultureInfo.InvariantCulture, out var parsedNum))
            {
                resolvedChapterNumber = parsedNum;
                if (string.IsNullOrEmpty(resolvedChapterSlug) && chapterRef.Length > match.Length)
                {
                    resolvedChapterSlug = chapterRef.Substring(match.Length).TrimStart('-');
                }
            }
            else
            {
                var fallbackMatch = System.Text.RegularExpressions.Regex.Match(chapterRef, @"(\d+(\.\d+)?)");
                if (fallbackMatch.Success && decimal.TryParse(fallbackMatch.Value, System.Globalization.CultureInfo.InvariantCulture, out var fbNum))
                {
                    resolvedChapterNumber = fbNum;
                }
            }
        }

        var chapter = await _chapterService.GetChapterForReadingAsync(storySlug, resolvedChapterNumber, resolvedChapterSlug, cancellationToken);
        if (chapter == null) return NotFound();

        // Save progress if authenticated
        if (User.Identity?.IsAuthenticated == true)
        {
            await _storyService.SaveReadingProgressAsync(chapter.StoryId, chapter.Id, 100, cancellationToken);
        }

        var allChapters = await _chapterService.GetChaptersByStoryIdAsync(chapter.StoryId, cancellationToken);
        var comments = await _commentService.GetStoryCommentsAsync(chapter.StoryId, chapter.Id, 1, 10, cancellationToken);

        var viewModel = new ReadingViewModel
        {
            Chapter = chapter,
            AllChapters = allChapters,
            InitialComments = comments
        };

        return View(viewModel);
    }

    public record SaveProgressRequest(int StoryId, int ChapterId, decimal Progress);

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveProgress([FromBody] SaveProgressRequest request, CancellationToken cancellationToken)
    {
        if (request == null) return BadRequest();
        var result = await _storyService.SaveReadingProgressAsync(request.StoryId, request.ChapterId, request.Progress, cancellationToken);
        return Json(new { success = result.Succeeded });
    }
}
