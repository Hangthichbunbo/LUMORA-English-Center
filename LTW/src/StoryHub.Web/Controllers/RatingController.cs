using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using StoryHub.Application.DTOs.Rating;
using StoryHub.Application.Interfaces;

namespace StoryHub.Web.Controllers;

public class RatingController : Controller
{
    private readonly IRatingService _ratingService;

    public RatingController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("rating")]
    public async Task<IActionResult> Submit([FromBody] SubmitRatingDto? dto, CancellationToken cancellationToken)
    {
        if (dto == null || dto.Score < 1 || dto.Score > 5 || dto.StoryId <= 0)
        {
            return BadRequest(new { success = false, message = "Dữ liệu đánh giá không hợp lệ (điểm từ 1 đến 5 sao)." });
        }

        var result = await _ratingService.SubmitRatingAsync(dto, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new { success = false, message = result.Errors.FirstOrDefault() ?? result.Message });
        }

        return Json(new { success = true, data = result.Data, message = result.Message });
    }
}
