namespace StoryHub.Application.Common.Options;

public class SecuritySettings
{
    public const string SectionName = "SecuritySettings";

    public int CommentEditWindowMinutes { get; set; } = 30;
    public bool EnableLockout { get; set; } = true;
    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutTimeSpanMinutes { get; set; } = 15;
}
