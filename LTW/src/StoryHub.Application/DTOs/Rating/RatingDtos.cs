namespace StoryHub.Application.DTOs.Rating;

public class SubmitRatingDto
{
    public int StoryId { get; set; }
    public int Score { get; set; } // 1 - 5
}

public class RatingSummaryDto
{
    public int StoryId { get; set; }
    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }
    public int? CurrentUserRating { get; set; }
}
