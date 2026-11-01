using System.Security.Claims;
using StoryHub.Application.Common.Interfaces;

namespace StoryHub.Web.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private HttpContext? HttpContext => _httpContextAccessor.HttpContext;

    public string? UserId => HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName => HttpContext?.User.FindFirstValue(ClaimTypes.Name);

    public bool IsAuthenticated => HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public IReadOnlyList<string> Roles => HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList() ?? [];

    public bool IsInRole(string role) => HttpContext?.User.IsInRole(role) ?? false;

    public string? IpAddress => HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => HttpContext?.Request.Headers.UserAgent.ToString();

    public string? CorrelationId => HttpContext?.TraceIdentifier;
}
