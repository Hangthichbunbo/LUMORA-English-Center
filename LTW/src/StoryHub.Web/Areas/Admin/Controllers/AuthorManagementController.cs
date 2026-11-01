using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.DTOs.Author;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AuthorManagementController : Controller
{
    private readonly IAuthorService _authorService;

    public AuthorManagementController(IAuthorService authorService)
    {
        _authorService = authorService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var authors = await _authorService.GetAllAuthorsAsync(cancellationToken);
        return View(authors);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Dữ liệu tác giả không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _authorService.CreateAuthorAsync(dto, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CreateOrUpdateAuthorDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Dữ liệu tác giả không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _authorService.UpdateAuthorAsync(dto, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _authorService.DeleteAuthorAsync(id, cancellationToken);
        if (result.Succeeded) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Index));
    }
}
