using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTW.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        public Skill Skill { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;

        // Đoạn văn cho Reading (không bắt buộc)
        public string? Passage { get; set; }

        // File audio đề bài cho Listening (không bắt buộc)
        [StringLength(500)]
        public string? AudioUrl { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal Points { get; set; } = 1;

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment? Assignment { get; set; }

        // Chỉ Listening/Reading có đáp án; Speaking/Writing để trống
        public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }
}
