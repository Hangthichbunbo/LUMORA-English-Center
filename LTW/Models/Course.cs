using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTW.Data;

namespace LTW.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên khóa học")]
        [StringLength(200)]
        [Display(Name = "Tên khóa học")]
        public string Title { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Mô tả ngắn")]
        public string ShortDescription { get; set; } = string.Empty;

        [Display(Name = "Mô tả chi tiết")]
        public string Description { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string Thumbnail { get; set; } = "/images/courses/default-course.jpg";

        [StringLength(50)]
        [Display(Name = "Trình độ")]
        public string Level { get; set; } = "All Levels"; // e.g. "IELTS 5.0 - 6.5", "Beginner", "IELTS 7.0+"

        [Required]
        [StringLength(50)]
        [Display(Name = "Danh mục")]
        public string Category { get; set; } = "IELTS"; // IELTS, Communication, Grammar, Listening, Reading, Writing, Speaking

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Học phí (VNĐ)")]
        public decimal Price { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Học phí gốc (VNĐ)")]
        public decimal OriginalPrice { get; set; } = 0;

        [StringLength(100)]
        [Display(Name = "Thời lượng")]
        public string Duration { get; set; } = "12 tuần";

        [Display(Name = "Giảng viên")]
        public string? TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public ApplicationUser? Teacher { get; set; }

        [Display(Name = "Đã xuất bản")]
        public bool IsPublished { get; set; } = true;

        [Display(Name = "Khóa học nổi bật")]
        public bool IsFeatured { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();
        public virtual ICollection<LiveClass> LiveClasses { get; set; } = new List<LiveClass>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        [NotMapped]
        public int TotalLessons => Chapters?.SelectMany(c => c.Lessons).Count() ?? 0;

        [NotMapped]
        public int TotalExercises => Chapters?.SelectMany(c => c.Lessons).SelectMany(l => l.Exercises).Count() ?? 0;
    }

    public class Chapter
    {
        [Key]
        public int ChapterId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên chương")]
        [StringLength(200)]
        [Display(Name = "Tên chương học")]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 1;

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }

    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        public int ChapterId { get; set; }

        [ForeignKey(nameof(ChapterId))]
        public Chapter? Chapter { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề bài học")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề bài học")]
        public string Title { get; set; } = string.Empty;

        public int OrderIndex { get; set; } = 1;

        [Display(Name = "Nội dung bài học")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Video URL")]
        public string? VideoUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Audio URL")]
        public string? AudioUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Tài liệu đính kèm (PDF/Doc)")]
        public string? DocumentUrl { get; set; }

        [Display(Name = "Từ vựng trọng tâm (JSON / Text)")]
        public string? Vocabulary { get; set; }

        [Display(Name = "Ngữ pháp cốt lõi")]
        public string? Grammar { get; set; }

        [Display(Name = "Cho phép học thử miễn phí")]
        public bool IsFreePreview { get; set; } = false;

        public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }

    public class LessonProgress
    {
        [Key]
        public int ProgressId { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey(nameof(LessonId))]
        public Lesson? Lesson { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        public DateTime LastAccessedAt { get; set; } = DateTime.Now;
    }
}

