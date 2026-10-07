using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTW.Models
{
    // Gộp Manager + Teacher + Student thành 1 bảng, phân biệt bằng Role
    public class UserAccount
    {
        [Key]
        public int UserAccountId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150)]
        public string? Email { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        public UserRole Role { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        // Chỉ dùng cho học viên (mỗi học viên thuộc 1 lớp)
        [Display(Name = "Lớp học")]
        public int? ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom? ClassRoom { get; set; }
    }
}
