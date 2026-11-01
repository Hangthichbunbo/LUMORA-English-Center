using FluentValidation;
using StoryHub.Application.DTOs.Advertisement;

namespace StoryHub.Application.Validators;

public class CreateOrUpdateAdDtoValidator : AbstractValidator<CreateOrUpdateAdDto>
{
    public CreateOrUpdateAdDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên quảng cáo không được để trống.")
            .MaximumLength(150).WithMessage("Tên quảng cáo tối đa 150 ký tự.");

        RuleFor(x => x.Priority)
            .GreaterThan(0).WithMessage("Độ ưu tiên phải lớn hơn 0.");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt)
            .WithMessage("Thời gian kết thúc phải diễn ra sau thời gian bắt đầu.");

        RuleFor(x => x.DestinationUrl)
            .Must(url => string.IsNullOrWhiteSpace(url) || (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Đường dẫn đích (Destination URL) bắt buộc phải sử dụng giao thức bảo mật HTTPS (https://) và không chứa script độc hại.");

        RuleFor(x => x.HtmlContent)
            .Must(html => string.IsNullOrWhiteSpace(html) || !html.Contains("<script", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Mã HTML quảng cáo không được chứa thẻ <script>.");
    }
}
