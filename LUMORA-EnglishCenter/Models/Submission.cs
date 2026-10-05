using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LUMORA_EnglishCenter.Models
{
    // Gộp HomeworkSubmission + TestAttempt: 1 lần nộp bài của học viên
    public class Submission
    {
        [Key]
        public int SubmissionId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Điểm tự động chấm (Listening + Reading)
        [Column(TypeName = "decimal(5,2)")]
        public decimal AutoScore { get; set; }

        // Điểm giáo viên chấm (Speaking + Writing), null = chưa chấm
        [Column(TypeName = "decimal(5,2)")]
        public decimal? TeacherScore { get; set; }

        [StringLength(1000)]
        public string? TeacherComment { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment? Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public UserAccount? Student { get; set; }

        public ICollection<SubmissionAnswer> Answers { get; set; } = new List<SubmissionAnswer>();
    }
}
