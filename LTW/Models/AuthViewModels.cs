using System.ComponentModel.DataAnnotations;

namespace LTW.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập")]
        [Display(Name = "Email hoặc Tên đăng nhập")]
        public string UsernameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false;

        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên từ 2 đến 100 ký tự", MinimumLength = 2)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50, ErrorMessage = "Tên đăng nhập từ 3 đến 50 ký tự", MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9_.-]+$", ErrorMessage = "Tên đăng nhập chỉ chứa chữ, số, dấu chấm hoặc gạch dưới")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối thiểu {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class VerifyOtpViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mã OTP")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã OTP gồm 6 chữ số")]
        [Display(Name = "Mã xác thực OTP")]
        public string OtpCode { get; set; } = string.Empty;

        public string Purpose { get; set; } = "Register"; // Register or ForgotPassword
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email đã đăng ký")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [StringLength(100, ErrorMessage = "Mật khẩu tối thiểu {2} ký tự", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }

    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Giới thiệu bản thân")]
        public string? Bio { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Vai trò người dùng")]
        public string RoleName { get; set; } = "Student";

        // Password change fields
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới (nếu đổi)")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmNewPassword { get; set; }
    }
}

