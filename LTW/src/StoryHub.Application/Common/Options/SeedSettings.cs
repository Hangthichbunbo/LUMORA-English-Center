namespace StoryHub.Application.Common.Options;

public class SeedSettings
{
    public const string SectionName = "SeedSettings";

    public bool SeedDefaultUsers { get; set; } = false;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string ModeratorEmail { get; set; } = string.Empty;
    public string ModeratorPassword { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserPassword { get; set; } = string.Empty;
}
