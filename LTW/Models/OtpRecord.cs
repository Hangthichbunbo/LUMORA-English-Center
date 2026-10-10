using System.ComponentModel.DataAnnotations;

namespace LTW.Models
{
    public class OtpRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string OtpCode { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Purpose { get; set; } = "Register"; // Register, ForgotPassword

        public string? PayloadJson { get; set; } // Stores pending registration info or token

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime ExpiresAt { get; set; } = DateTime.Now.AddMinutes(15);

        public bool IsUsed { get; set; } = false;
    }
}

