namespace StoryHub.Web.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Strip technology fingerprinting headers
        headers.Remove("Server");
        headers.Remove("X-Powered-By");

        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Remove("Server");
            context.Response.Headers.Remove("X-Powered-By");
            return Task.CompletedTask;
        });

        // Content-Security-Policy (CSP)
        if (!headers.ContainsKey("Content-Security-Policy"))
        {
            headers.Append("Content-Security-Policy",
                "default-src 'self'; " +
                "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
                "font-src 'self' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com data:; " +
                "img-src 'self' data: https: blob:; " +
                "connect-src 'self'; " +
                "frame-src 'self' data: blob:; " +
                "frame-ancestors 'none'; " +
                "form-action 'self'; " +
                "base-uri 'self'; " +
                "object-src 'none';");
        }

        // Prevent MIME type sniffing
        if (!headers.ContainsKey("X-Content-Type-Options"))
        {
            headers.Append("X-Content-Type-Options", "nosniff");
        }

        // Prevent clickjacking by denying iframing
        if (!headers.ContainsKey("X-Frame-Options"))
        {
            headers.Append("X-Frame-Options", "DENY");
        }

        // Strict Referrer Policy
        if (!headers.ContainsKey("Referrer-Policy"))
        {
            headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        // Restrict unnecessary browser hardware features
        if (!headers.ContainsKey("Permissions-Policy"))
        {
            headers.Append("Permissions-Policy", "camera=(), microphone=(), geolocation=(), payment=(), usb=(), vr=()");
        }

        // Cross-Origin Isolation & Boundary Protection
        if (!headers.ContainsKey("Cross-Origin-Opener-Policy"))
        {
            headers.Append("Cross-Origin-Opener-Policy", "same-origin");
        }

        if (!headers.ContainsKey("Cross-Origin-Resource-Policy"))
        {
            var reqPath = context.Request.Path.Value ?? string.Empty;
            var isPublicAsset = reqPath.StartsWith("/favicon", StringComparison.OrdinalIgnoreCase) ||
                                reqPath.StartsWith("/images", StringComparison.OrdinalIgnoreCase) ||
                                reqPath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase) ||
                                reqPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                reqPath.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ||
                                reqPath.EndsWith(".webmanifest", StringComparison.OrdinalIgnoreCase);

            // Favicons and public assets need 'cross-origin' so browser tab shells and search engines can render them
            headers.Append("Cross-Origin-Resource-Policy", isPublicAsset ? "cross-origin" : "same-origin");
        }

        // Disallow Adobe Flash / Acrobat cross-domain policy files
        if (!headers.ContainsKey("X-Permitted-Cross-Domain-Policies"))
        {
            headers.Append("X-Permitted-Cross-Domain-Policies", "none");
        }

        // Modern browsers standard: disable obsolete XSS auditor that could introduce vulnerabilities
        if (!headers.ContainsKey("X-XSS-Protection"))
        {
            headers.Append("X-XSS-Protection", "0");
        }

        await _next(context);
    }
}
