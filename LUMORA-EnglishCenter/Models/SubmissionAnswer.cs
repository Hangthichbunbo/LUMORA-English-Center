using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LUMORA_EnglishCenter.Models
{
    public class SubmissionAnswer
    {
        [Key]
        public int SubmissionAnswerId { get; set; }

        // Listening / Reading: đáp án đã chọn (tự chấm bằng AnswerOption.IsCorrect)
        public int? SelectedOptionId { get; set; }

        [ForeignKey(nameof(SelectedOptionId))]
        public AnswerOption? SelectedOption { get; set; }

        // Writing: bài viết của học viên
        public string? AnswerText { get; set; }

        // Speaking: đường dẫn file ghi âm
        [StringLength(500)]
        public string? AudioUrl { get; set; }

        [Required]
        public int SubmissionId { get; set; }

        [ForeignKey(nameof(SubmissionId))]
        public Submission? Submission { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question? Question { get; set; }
    }
}
