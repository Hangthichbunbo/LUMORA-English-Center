using StoryHub.Application.DTOs.Story;

namespace StoryHub.Application.DTOs.Dashboard;

public class AdminDashboardDto
{
    public int TotalStories { get; set; }
    public int TotalChapters { get; set; }
    public int TotalUsers { get; set; }
    public int TotalComments { get; set; }
    public long TotalViews { get; set; }
    public int PublishedStoriesCount { get; set; }
    public int HiddenOrRemovedStoriesCount { get; set; }
    public int PendingCommentsCount { get; set; }
    public int PendingReportsCount { get; set; }
    public int ActiveAdsCount { get; set; }
    public List<StoryDto> RecentStories { get; set; } = [];
    public List<DailyViewStatDto> DailyViewStats { get; set; } = [];
}

public class DailyViewStatDto
{
    public string Date { get; set; } = string.Empty;
    public long Views { get; set; }
}
