using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LUMORA_EnglishCenter.Models
{
    // Gộp Homework + Test: bài tập (1-4 kỹ năng) hoặc bài test (4 kỹ năng)
    // Kỹ năng nằm ở từng Question, nên 1 Assignment có thể chứa bất kỳ kỹ năng nào
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Là bài test")]
        public bool IsTest { get; set; }   // false = bài tập, true = test

        [Display(Name = "Hạn nộp")]
        public DateTime? Deadline { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom? ClassRoom { get; set; }

        public ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
}
