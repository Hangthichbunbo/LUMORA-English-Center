using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AuditLogController : Controller
{
    private readonly IAuditService _auditService;

    public AuditLogController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? entityName,
        string? actionName,
        string? userId,
        int pageIndex = 1,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditService.GetAuditLogsPaginatedAsync(entityName, actionName, userId, pageIndex, 25, cancellationToken);
        ViewData["EntityName"] = entityName;
        ViewData["ActionName"] = actionName;
        ViewData["UserId"] = userId;

        return View(logs);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(long id, CancellationToken cancellationToken)
    {
        var log = await _auditService.GetAuditLogByIdAsync(id, cancellationToken);
        if (log == null) return NotFound();

        return View(log);
    }
}
