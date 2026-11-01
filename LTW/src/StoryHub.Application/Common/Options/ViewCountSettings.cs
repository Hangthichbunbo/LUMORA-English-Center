namespace StoryHub.Application.Common.Options;

public class ViewCountSettings
{
    public const string SectionName = "ViewCountSettings";

    public int ThrottleMinutes { get; set; } = 10;
    public int FlushIntervalSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 500;
}
