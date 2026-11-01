using Ganss.Xss;
using StoryHub.Application.Common.Interfaces;

namespace StoryHub.Infrastructure.Services;

public class GanssHtmlSanitizationService : IHtmlSanitizationService
{
    private readonly HtmlSanitizer _sanitizer;

    public GanssHtmlSanitizationService()
    {
        _sanitizer = new HtmlSanitizer();

        // Configure allowed tags strictly
        _sanitizer.AllowedTags.Clear();
        var safeTags = new[]
        {
            "p", "br", "strong", "em", "b", "i", "u", "s", "span", "div",
            "blockquote", "ul", "ol", "li", "h1", "h2", "h3", "h4", "h5", "h6",
            "a", "img", "table", "thead", "tbody", "tfoot", "tr", "th", "td", "sub", "sup"
        };
        foreach (var tag in safeTags) _sanitizer.AllowedTags.Add(tag);

        // Configure allowed attributes
        _sanitizer.AllowedAttributes.Clear();
        var safeAttributes = new[] { "href", "src", "alt", "title", "class", "target", "rel", "style", "colspan", "rowspan", "scope" };
        foreach (var attr in safeAttributes) _sanitizer.AllowedAttributes.Add(attr);

        // Allowed schemes (Strictly HTTP/HTTPS)
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");

        // Hook to force rel="noopener noreferrer sponsored" on <a> tags
        _sanitizer.PostProcessNode += (s, e) =>
        {
            if (e.Node is AngleSharp.Html.Dom.IHtmlAnchorElement anchor)
            {
                anchor.SetAttribute("rel", "noopener noreferrer sponsored");
                if (string.IsNullOrEmpty(anchor.Target))
                {
                    anchor.Target = "_blank";
                }
            }
        };
    }

    public string SanitizeHtml(string? rawHtml)
    {
        if (string.IsNullOrWhiteSpace(rawHtml)) return string.Empty;
        return _sanitizer.Sanitize(rawHtml);
    }

    public bool IsSafeUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        // Block dangerous schemes immediately
        var lower = url.Trim().ToLowerInvariant();
        if (lower.StartsWith("javascript:") || lower.StartsWith("data:") || lower.StartsWith("vbscript:"))
        {
            return false;
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp;
        }

        // Relative path
        return url.StartsWith('/') && !url.StartsWith("//");
    }
}
