using StoryHub.Application.DTOs.Dashboard;

namespace StoryHub.Application.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
}
