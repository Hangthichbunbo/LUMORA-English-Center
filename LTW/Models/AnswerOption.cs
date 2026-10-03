using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTW_5.Models
{
    public class AnswerOption
    {
        [Key]
        public int AnswerOptionId { get; set; }

        [Required]
        [StringLength(500)]
        public string Text { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [ForeignKey(nameof(QuestionId))]
        public Question? Question { get; set; }
    }
}
