using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LTW.Data
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? AvatarUrl { get; set; } = "/images/avatars/default.png";

        [StringLength(30)]
        public string RoleName { get; set; } = "Student"; // "Student", "Teacher", "Admin"

        [StringLength(500)]
        [Display(Name = "Giới thiệu bản thân")]
        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}