using System.Text.RegularExpressions;
using FluentValidation;
using StoryHub.Application.DTOs.Chapter;

namespace StoryHub.Application.Validators;

public class CreateChapterDtoValidator : AbstractValidator<CreateChapterDto>
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public CreateChapterDtoValidator()
    {
        RuleFor(x => x.StoryId)
            .GreaterThan(0).WithMessage("Truyện không hợp lệ.");

        RuleFor(x => x.ChapterNumber)
            .GreaterThan(0).WithMessage("Số thứ tự chương phải lớn hơn 0.")
            .Must(x => x == Math.Floor(x) && x >= 1).WithMessage("Số thứ tự chương phải là số nguyên dương (1, 2, 3...).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Tiêu đề chương không được để trống.")
            .MaximumLength(250).WithMessage("Tiêu đề chương tối đa 250 ký tự.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug chương không được để trống.")
            .MaximumLength(260).WithMessage("Slug tối đa 260 ký tự.")
            .Must(s => string.IsNullOrEmpty(s) || SlugRegex.IsMatch(s))
            .WithMessage("Slug chương chỉ được chứa chữ thường (a-z), số (0-9) và dấu gạch ngang (-).");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Nội dung chương không được để trống.")
            .MinimumLength(10).WithMessage("Nội dung chương phải có ít nhất 10 ký tự.")
            .Must(ValidateChapterInlineImages).WithMessage("Có hình ảnh đính kèm trong nội dung chương vượt quá dung lượng tối đa 1 MB.");
    }

    private static bool ValidateChapterInlineImages(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return true;

        const long maxBytes = 1 * 1024 * 1024; // 1 MB
        var base64Matches = Regex.Matches(content, @"<img\s+[^>]*?src=[""']data:image\/[^;]+;base64,([A-Za-z0-9+/=\s]+)[""']", RegexOptions.IgnoreCase);
        foreach (Match match in base64Matches)
        {
            var clean = match.Groups[1].Value.Trim().Replace(" ", "").Replace("\r", "").Replace("\n", "");
            long approxBytes = (clean.Length * 3) / 4;
            if (approxBytes > maxBytes)
            {
                return false;
            }
        }
        return true;
    }
}

public class UpdateChapterDtoValidator : AbstractValidator<UpdateChapterDto>
{
    public UpdateChapterDtoValidator()
    {
        Include(new CreateChapterDtoValidator());

        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Id chương không hợp lệ.");

        RuleFor(x => x.RowVersion)
            .NotEmpty().WithMessage("Mã phiên bản (RowVersion) không được để trống.");
    }
}
