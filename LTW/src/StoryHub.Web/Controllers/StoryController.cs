using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.DTOs.Story;
using StoryHub.Application.Interfaces;
using StoryHub.Web.ViewModels;

namespace StoryHub.Web.Controllers;

public class StoryController : Controller
{
    private readonly IStoryService _storyService;
    private readonly IChapterService _chapterService;
    private readonly ICommentService _commentService;
    private readonly ICategoryService _categoryService;
    private readonly IAuthorService _authorService;

    public StoryController(
        IStoryService storyService,
        IChapterService chapterService,
        ICommentService commentService,
        ICategoryService categoryService,
        IAuthorService authorService)
    {
        _storyService = storyService;
        _chapterService = chapterService;
        _commentService = commentService;
        _categoryService = categoryService;
        _authorService = authorService;
    }

    [HttpGet("truyen")]
    public async Task<IActionResult> Index([FromQuery] StoryFilterDto filter, CancellationToken cancellationToken)
    {
        var stories = await _storyService.GetStoriesPaginatedAsync(filter, cancellationToken);
        var categories = await _categoryService.GetAllCategoriesAsync(true, cancellationToken);
        var authors = await _authorService.GetAllAuthorsAsync(cancellationToken);

        var currentCategory = !string.IsNullOrEmpty(filter.CategorySlug)
            ? categories.FirstOrDefault(c => c.Slug == filter.CategorySlug)
            : null;

        var currentAuthor = !string.IsNullOrEmpty(filter.AuthorSlug)
            ? authors.FirstOrDefault(a => a.Slug == filter.AuthorSlug)
            : null;

        string pageTitle = "Danh sách truyện";
        if (currentCategory != null) pageTitle = $"Thể loại: {currentCategory.Name}";
        else if (currentAuthor != null) pageTitle = $"Tác giả: {currentAuthor.Name}";
        else if (!string.IsNullOrWhiteSpace(filter.SearchTerm)) pageTitle = $"Kết quả tìm kiếm: \"{filter.SearchTerm}\"";

        var viewModel = new StoryListViewModel
        {
            Stories = stories,
            Filter = filter,
            Categories = categories,
            Authors = authors,
            CurrentCategory = currentCategory,
            CurrentAuthor = currentAuthor,
            PageTitle = pageTitle
        };

        return View(viewModel);
    }

    [HttpGet("truyen/{slug}")]
    public async Task<IActionResult> Detail(string slug, CancellationToken cancellationToken)
    {
        var story = await _storyService.GetStoryBySlugAsync(slug, cancellationToken);
        if (story == null) return NotFound();

        var chapters = await _chapterService.GetChaptersByStoryIdAsync(story.Id, cancellationToken);
        var initialComments = await _commentService.GetStoryCommentsAsync(story.Id, null, 1, 10, cancellationToken);

        var viewModel = new StoryDetailViewModel
        {
            Story = story,
            Chapters = chapters,
            InitialComments = initialComments
        };

        return View(viewModel);
    }

    public record ToggleFollowRequest(int StoryId);

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFollow([FromBody] ToggleFollowRequest request, CancellationToken cancellationToken)
    {
        if (request == null || request.StoryId <= 0)
        {
            return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _storyService.ToggleFollowStoryAsync(request.StoryId, cancellationToken);
        return Json(new { success = result.Succeeded, data = result.Data, message = result.Message });
    }
}
