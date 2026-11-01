using FluentValidation;
using StoryHub.Application.DTOs.Comment;

namespace StoryHub.Application.Validators;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleFor(x => x.StoryId)
            .GreaterThan(0).WithMessage("Truyện bình luận không hợp lệ.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống.")
            .MinimumLength(2).WithMessage("Bình luận phải có ít nhất 2 ký tự.")
            .MaximumLength(1000).WithMessage("Bình luận tối đa 1000 ký tự.")
            .Must(c => !c.Contains("<script", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Bình luận chứa thẻ script không hợp lệ.");
    }
}

public class UpdateCommentDtoValidator : AbstractValidator<UpdateCommentDto>
{
    public UpdateCommentDtoValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("Id bình luận không hợp lệ.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung bình luận không được để trống.")
            .MinimumLength(2).WithMessage("Bình luận phải có ít nhất 2 ký tự.")
            .MaximumLength(1000).WithMessage("Bình luận tối đa 1000 ký tự.")
            .Must(c => !c.Contains("<script", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Bình luận chứa thẻ script không hợp lệ.");
    }
}

public class ReportCommentDtoValidator : AbstractValidator<ReportCommentDto>
{
    public ReportCommentDtoValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0).WithMessage("Bình luận được báo cáo không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Vui lòng chọn lý do báo cáo.")
            .MaximumLength(100).WithMessage("Lý do báo cáo tối đa 100 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Chi tiết báo cáo tối đa 500 ký tự.");
    }
}
