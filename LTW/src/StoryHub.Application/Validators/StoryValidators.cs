using System.Text.RegularExpressions;
using FluentValidation;
using StoryHub.Application.DTOs.Story;

namespace StoryHub.Application.Validators;

public class CreateStoryDtoValidator : AbstractValidator<CreateStoryDto>
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public CreateStoryDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề truyện không được để trống.")
            .MaximumLength(250).WithMessage("Tiêu đề truyện tối đa 250 ký tự.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Đường dẫn tĩnh (Slug) không được để trống.")
            .MaximumLength(260).WithMessage("Slug tối đa 260 ký tự.")
            .Must(s => string.IsNullOrEmpty(s) || SlugRegex.IsMatch(s))
            .WithMessage("Slug chỉ được chứa chữ thường (a-z), chữ số (0-9) và dấu gạch ngang (-).");

        RuleFor(x => x)
            .Must(x => x.AuthorId > 0 || !string.IsNullOrWhiteSpace(x.AuthorName))
            .WithMessage("Vui lòng chọn hoặc nhập tên tác giả.")
            .WithName("AuthorName");

        When(x => !string.IsNullOrWhiteSpace(x.AuthorName), () =>
        {
            RuleFor(x => x.AuthorName)
                .MaximumLength(150).WithMessage("Tên tác giả tối đa 150 ký tự.");
        });

        RuleFor(x => x.CategoryIds)
            .NotEmpty().WithMessage("Truyện phải thuộc ít nhất một thể loại.");

        RuleFor(x => x.Description)
            .MaximumLength(4000).WithMessage("Mô tả truyện tối đa 4000 ký tự.");
    }
}

public class UpdateStoryDtoValidator : AbstractValidator<UpdateStoryDto>
{
    public UpdateStoryDtoValidator()
    {
        Include(new CreateStoryDtoValidator());

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id truyện không hợp lệ.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Mã phiên bản (RowVersion) không được để trống.");
    }
}

public class RemoveStoryDtoValidator : AbstractValidator<RemoveStoryDto>
{
    public RemoveStoryDtoValidator()
    {
        RuleFor(x => x.StoryId)
            .GreaterThan(0).WithMessage("Id truyện không hợp lệ.");

        RuleFor(x => x.RemovalReason)
            .NotEmpty().WithMessage("Lý do gỡ truyện là bắt buộc.")
            .MinimumLength(5).WithMessage("Lý do gỡ truyện phải có ít nhất 5 ký tự.")
            .MaximumLength(500).WithMessage("Lý do gỡ truyện tối đa 500 ký tự.");
    }
}
