using FluentValidation;
using StoryHub.Application.DTOs.Rating;

namespace StoryHub.Application.Validators;

public class SubmitRatingDtoValidator : AbstractValidator<SubmitRatingDto>
{
    public SubmitRatingDtoValidator()
    {
        RuleFor(x => x.StoryId)
            .GreaterThan(0).WithMessage("Truyện không hợp lệ.");

        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("Điểm đánh giá phải từ 1 đến 5 sao.");
    }
}
