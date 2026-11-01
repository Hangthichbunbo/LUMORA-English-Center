using System.ComponentModel.DataAnnotations;
using StoryHub.Application.Common.Models;
using StoryHub.Application.DTOs.Author;
using StoryHub.Application.DTOs.Category;
using StoryHub.Application.DTOs.Chapter;
using StoryHub.Application.DTOs.Comment;
using StoryHub.Application.DTOs.Story;

namespace StoryHub.Web.ViewModels;

public class HomeViewModel
{
    public List<StoryDto> FeaturedStories { get; set; } = [];
    public List<StoryDto> LatestStories { get; set; } = [];
    public List<StoryDto> TopViewedStories { get; set; } = [];
    public List<StoryDto> TopRatedStories { get; set; } = [];
    public List<StoryDto> CompletedStories { get; set; } = [];
    public List<CategoryDto> Categories { get; set; } = [];
}

public class StoryListViewModel
{
    public PaginatedList<StoryDto> Stories { get; set; } = null!;
    public StoryFilterDto Filter { get; set; } = new();
    public List<CategoryDto> Categories { get; set; } = [];
    public List<AuthorDto> Authors { get; set; } = [];
    public CategoryDto? CurrentCategory { get; set; }
    public AuthorDto? CurrentAuthor { get; set; }
    public string PageTitle { get; set; } = "Danh sách truyện";
}

public class StoryDetailViewModel
{
    public StoryDetailDto Story { get; set; } = null!;
    public List<ChapterDto> Chapters { get; set; } = [];
    public PaginatedList<CommentDto> InitialComments { get; set; } = null!;
}

public class ReadingViewModel
{
    public ChapterDetailDto Chapter { get; set; } = null!;
    public List<ChapterDto> AllChapters { get; set; } = [];
    public PaginatedList<CommentDto> InitialComments { get; set; } = null!;
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}

public class RegisterViewModel
{
    [Required(ErrorMessage = "Tên hiển thị là bắt buộc.")]
    [StringLength(15, MinimumLength = 2, ErrorMessage = "Tên hiển thị từ 2 đến 15 ký tự.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
    [GoogleEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã xác nhận.")]
    [Display(Name = "Mã xác nhận")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Mã xác nhận không hợp lệ.")]
    public string CaptchaCode { get; set; } = string.Empty;
}

public class VerifyAuthenticatorViewModel
{
    [Required]
    public string SetupToken { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string SharedKey { get; set; } = string.Empty;

    public string FormattedKey { get; set; } = string.Empty;

    public string AuthenticatorUri { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã xác thực 6 số từ Google Authenticator.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác thực phải gồm đúng 6 chữ số.")]
    [Display(Name = "Mã xác thực 6 số")]
    public string Code { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
    [GoogleEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã xác nhận.")]
    [Display(Name = "Mã xác nhận")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "Mã xác nhận không hợp lệ.")]
    public string CaptchaCode { get; set; } = string.Empty;
}

public class ResetPasswordViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
    [GoogleEmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mã xác thực từ Google Authenticator.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác thực phải gồm đúng 6 chữ số.")]
    [Display(Name = "Mã Google Authenticator")]
    public string AuthenticatorCode { get; set; } = string.Empty;
}

public class SetNewPasswordViewModel
{
    [Required]
    public string ResetToken { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không trùng khớp.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public static class GoogleEmailValidator
{
    public const string DefaultErrorMessage = "Hệ thống chỉ chấp nhận tài khoản Google (@gmail.com hoặc @googlemail.com).";

    public static bool IsValidGoogleEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var trimmed = email.Trim();
        var atIndex = trimmed.IndexOf('@');
        if (atIndex <= 0) return false;

        // Ensure there is only one '@' symbol
        if (trimmed.IndexOf('@', atIndex + 1) != -1) return false;

        var localPart = trimmed[..atIndex];
        var domainPart = trimmed[(atIndex + 1)..];

        if (string.IsNullOrWhiteSpace(localPart) || string.IsNullOrWhiteSpace(domainPart)) return false;

        // Local part cannot start or end with '.'
        if (localPart.StartsWith('.') || localPart.EndsWith('.')) return false;

        return domainPart.Equals("gmail.com", StringComparison.OrdinalIgnoreCase)
            || domainPart.Equals("googlemail.com", StringComparison.OrdinalIgnoreCase);
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class GoogleEmailAddressAttribute : ValidationAttribute
{
    public GoogleEmailAddressAttribute()
        : base(GoogleEmailValidator.DefaultErrorMessage)
    {
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success;
        }

        var email = value.ToString()!;
        if (!GoogleEmailValidator.IsValidGoogleEmail(email))
        {
            var memberNames = validationContext.MemberName != null
                ? new[] { validationContext.MemberName }
                : Array.Empty<string>();
            return new ValidationResult(ErrorMessageString, memberNames);
        }

        return ValidationResult.Success;
    }
}

public class ProfileViewModel
{
    [Required(ErrorMessage = "Tên hiển thị là bắt buộc.")]
    [StringLength(15, MinimumLength = 2, ErrorMessage = "Tên hiển thị từ 2 đến 15 ký tự.")]
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTimeOffset? CommentBlockedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mật khẩu hiện tại là bắt buộc.")]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu mới là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu mới tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không trùng khớp.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
