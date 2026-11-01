namespace StoryHub.Application.Common.Interfaces;

public interface IHtmlSanitizationService
{
    string SanitizeHtml(string? rawHtml);
    bool IsSafeUrl(string? url);
}
