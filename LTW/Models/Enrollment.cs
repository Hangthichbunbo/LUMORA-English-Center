using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTW.Data;

namespace LTW.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Số tiền thanh toán")]
        public decimal PricePaid { get; set; }

        [StringLength(50)]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "VNPay"; // VNPay, MoMo, BankTransfer, CreditCard

        [StringLength(50)]
        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; } = "Completed"; // Completed, Pending, Cancelled

        [StringLength(100)]
        [Display(Name = "Mã giao dịch")]
        public string TransactionId { get; set; } = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
    }
}

