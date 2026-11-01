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
    }
}

