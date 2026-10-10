using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTW.Data;

namespace LTW.Models
{
    public class Test
    {
        [Key]
        public int TestId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài kiểm tra")]
        [StringLength(200)]
        [Display(Name = "Tên bài kiểm tra")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Loại bài kiểm tra")]
        public ExamType Type { get; set; } = ExamType.ExampleTest; // ExampleTest (Free) or PlacementTest (Placement)

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string Thumbnail { get; set; } = "/images/tests/default-test.jpg";

        [Display(Name = "Thời gian làm bài (Phút)")]
        public int TimeLimitMinutes { get; set; } = 45;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<TestQuestion> Questions { get; set; } = new List<TestQuestion>();
        public virtual ICollection<TestAttempt> Attempts { get; set; } = new List<TestAttempt>();
    }

    public class TestQuestion
    {
        [Key]
        public int TestQuestionId { get; set; }

        [Required]
        public int TestId { get; set; }

        [ForeignKey(nameof(TestId))]
        public Test? Test { get; set; }

        [Display(Name = "Kỹ năng kiểm tra")]
        public SkillType Skill { get; set; } = SkillType.Listening; // Listening, Reading, Writing, Speaking

        [Display(Name = "Dạng câu hỏi")]
        public QuestionFormat Format { get; set; } = QuestionFormat.MultipleChoice;

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi")]
        [Display(Name = "Nội dung câu hỏi / Đề bài")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Đoạn văn bài đọc (Reading passage)")]
        public string? Passage { get; set; }

        [StringLength(500)]
        [Display(Name = "Đường dẫn File Audio (Listening)")]
        public string? AudioUrl { get; set; }

        [Display(Name = "Các lựa chọn đáp án (JSON Array)")]
        public string? OptionsJson { get; set; } // ["A. ...", "B. ...", "C. ...", "D. ..."]

        [StringLength(500)]
        [Display(Name = "Đáp án chuẩn")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Điểm số")]
        public decimal Points { get; set; } = 1;

        [Display(Name = "Giải thích chi tiết")]
        public string? Explanation { get; set; }
    }

    public class TestAttempt
    {
        [Key]
        public int AttemptId { get; set; }

        [Required]
        public int TestId { get; set; }

        [ForeignKey(nameof(TestId))]
        public Test? Test { get; set; }

        public string? StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [StringLength(100)]
        [Display(Name = "Họ và tên thí sinh")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Email thí sinh")]
        public string Email { get; set; } = string.Empty;

        public DateTime StartedAt { get; set; } = DateTime.Now;

        public DateTime CompletedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(4,1)")]
        [Display(Name = "Listening Band")]
        public decimal ListeningScore { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        [Display(Name = "Reading Band")]
        public decimal ReadingScore { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        [Display(Name = "Writing Band")]
        public decimal WritingScore { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        [Display(Name = "Speaking Band")]
        public decimal SpeakingScore { get; set; }

        [Column(TypeName = "decimal(4,1)")]
        [Display(Name = "Overall Band Score")]
        public decimal OverallScore { get; set; }

        [StringLength(100)]
        [Display(Name = "Trình độ ước tính")]
        public string EstimatedLevel { get; set; } = "B2 - Upper Intermediate (IELTS 6.5)";

        [Display(Name = "Đánh giá chi tiết & Nhận xét")]
        public string Feedback { get; set; } = string.Empty;

        [Display(Name = "Khóa học gợi ý (JSON)")]
        public string? RecommendedCoursesJson { get; set; }

        [Display(Name = "Chi tiết câu trả lời (JSON)")]
        public string? AnswersJson { get; set; }
    }
}

