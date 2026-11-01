using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StoryHub.Web.Controllers;

public class ErrorController : Controller
{
    [HttpGet("Error/{statusCode:int}")]
    public IActionResult HttpError(int statusCode, string? traceId = null)
    {
        Response.StatusCode = statusCode;
        ViewData["TraceId"] = traceId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        return statusCode switch
        {
            403 => View("403"),
            404 => View("404"),
            429 => View("429"),
            _ => View("500")
        };
    }
}
