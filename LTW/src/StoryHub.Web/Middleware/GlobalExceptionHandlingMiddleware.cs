using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace StoryHub.Web.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate _next,
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IHostEnvironment env)
    {
        this._next = _next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        _logger.LogError(
            ex,
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}, User: {User}",
            traceId,
            context.Request.Path,
            context.Request.Method,
            context.User.Identity?.Name ?? "Anonymous");

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot write error response for TraceId: {TraceId}", traceId);
            return;
        }

        var isApiOrAjax = context.Request.Headers.XRequestedWith == "XMLHttpRequest"
            || (context.Request.Headers.Accept.ToString().Contains(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase))
            || (context.Request.ContentType?.Contains(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase) == true)
            || context.Request.Path.StartsWithSegments("/api");

        if (isApiOrAjax)
        {
            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json; charset=utf-8";

            var problem = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Đã xảy ra lỗi máy chủ nội bộ.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = (_env.IsDevelopment() || _env.IsEnvironment("Testing"))
                    ? $"{ex.Message}\n{ex.StackTrace}"
                    : "Hệ thống gặp sự cố trong quá trình xử lý yêu cầu. Vui lòng thử lại sau.",
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = traceId;

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _env.IsDevelopment()
            });

            await context.Response.WriteAsync(json);
        }
        else
        {
            // For standard HTML page requests, re-route to error controller
            context.Response.Redirect($"/Error/500?traceId={traceId}");
        }
    }
}
