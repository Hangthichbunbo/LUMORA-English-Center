using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTW.Data;

namespace LTW.Models
{
    public class Exercise
    {
        [Key]
        public int ExerciseId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public Lesson? Lesson { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài tập")]
        [StringLength(200)]
        [Display(Name = "Tên bài tập")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Hướng dẫn làm bài")]
        public string? Description { get; set; }

        [Display(Name = "Thời gian làm bài (Phút)")]
        public int TimeLimitMinutes { get; set; } = 30;

        [Display(Name = "Điểm đạt (trên 100)")]
        public int PassingScore { get; set; } = 70;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<ExerciseQuestion> Questions { get; set; } = new List<ExerciseQuestion>();
        public virtual ICollection<ExerciseSubmission> Submissions { get; set; } = new List<ExerciseSubmission>();

        [NotMapped]
        public bool HasWriting => Questions != null && Questions.Any(q => q.Format == QuestionFormat.Writing || q.Format == QuestionFormat.Speaking);

        [NotMapped]
        public bool HasMultipleChoice => Questions != null && Questions.Any(q => q.Format != QuestionFormat.Writing && q.Format != QuestionFormat.Speaking);

        [NotMapped]
        public decimal TotalMcPoints => Questions != null ? Questions.Where(q => q.Format != QuestionFormat.Writing && q.Format != QuestionFormat.Speaking).Sum(q => q.Points > 0 ? q.Points : 10m) : 0m;
    }

    public class ExerciseQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [ForeignKey(nameof(ExerciseId))]
        public Exercise? Exercise { get; set; }

        public SkillType Skill { get; set; } = SkillType.Reading;

        public QuestionFormat Format { get; set; } = QuestionFormat.MultipleChoice;

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Đoạn văn đọc hiểu (nếu có)")]
        public string? Passage { get; set; }

        [StringLength(500)]
        [Display(Name = "Audio URL (nếu có)")]
        public string? AudioUrl { get; set; }

        [Display(Name = "Các lựa chọn (JSON Array)")]
        public string? OptionsJson { get; set; } // e.g. ["A. Option 1", "B. Option 2", "C. Option 3", "D. Option 4"]

        [Required(ErrorMessage = "Vui lòng nhập đáp án đúng")]
        [StringLength(500)]
        [Display(Name = "Đáp án đúng")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Display(Name = "Giải thích chi tiết")]
        public string? Explanation { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm câu hỏi")]
        public decimal Points { get; set; } = 1;
    }

    public class ExerciseSubmission
    {
        [Key]
        public int SubmissionId { get; set; }

        [Required]
        public int ExerciseId { get; set; }

        [ForeignKey(nameof(ExerciseId))]
        public Exercise? Exercise { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm tự chấm")]
        public decimal AutoScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm giáo viên chấm")]
        public decimal? TeacherScore { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Tổng điểm")]
        public decimal TotalScore { get; set; }

        [StringLength(1000)]
        [Display(Name = "Nhận xét của giáo viên")]
        public string? TeacherComment { get; set; }

        [Display(Name = "Chi tiết bài làm (JSON)")]
        public string? AnswersJson { get; set; } // Saves user chosen answers, correctness, feedback

        [NotMapped]
        public bool HasWriting => Exercise?.HasWriting ?? false;

        [NotMapped]
        public bool HasMultipleChoice => Exercise?.HasMultipleChoice ?? false;

        [NotMapped]
        public decimal TotalMcPoints => Exercise?.TotalMcPoints ?? 0m;

        [NotMapped]
        public string McScoreDisplay
        {
            get
            {
                var total = TotalMcPoints;
                if (total <= 0)
                {
                    return (AutoScore % 1 == 0) ? ((int)AutoScore).ToString() : AutoScore.ToString("F1");
                }
                string earnedStr = (AutoScore % 1 == 0) ? ((int)AutoScore).ToString() : AutoScore.ToString("F1");
                string totalStr = (total % 1 == 0) ? ((int)total).ToString() : total.ToString("F1");
                return $"{earnedStr}/{totalStr}";
            }
        }
    }
    public class QuestionInputModel
    {
        public int QuestionId { get; set; }
        public string Format { get; set; } = "MultipleChoice"; // "MultipleChoice" or "Writing"
        public SkillType Skill { get; set; } = SkillType.Reading;
        public string Content { get; set; } = string.Empty;
        public string? OptA { get; set; }
        public string? OptB { get; set; }
        public string? OptC { get; set; }
        public string? OptD { get; set; }
        public string? CorrectAnswer { get; set; }
        public string? WritingAnswer { get; set; }
        public string? Explanation { get; set; }
        public decimal Points { get; set; } = 10;
    }
}

