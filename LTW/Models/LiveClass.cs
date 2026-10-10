using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LTW.Data;

namespace LTW.Models
{
    public class LiveClass
    {
        [Key]
        public int LiveClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
        [StringLength(150)]
        [Display(Name = "Tên lớp học trực tuyến")]
        public string ClassName { get; set; } = string.Empty;

        [Display(Name = "Khóa học liên kết")]
        public int? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }

        [Required]
        [Display(Name = "Giáo viên phụ trách")]
        public string TeacherId { get; set; } = string.Empty;

        [ForeignKey(nameof(TeacherId))]
        public ApplicationUser? Teacher { get; set; }

        [Display(Name = "Ngày học")]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;

        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan StartTime { get; set; } = new TimeSpan(19, 30, 0);

        [Display(Name = "Giờ kết thúc")]
        public TimeSpan EndTime { get; set; } = new TimeSpan(21, 0, 0);

        [StringLength(500)]
        [Display(Name = "Phòng học trực tuyến (URL / Code)")]
        public string LiveRoomUrl { get; set; } = "https://meet.lumora.edu.vn/room-ielts";

        [Display(Name = "Sĩ số tối đa")]
        public int Capacity { get; set; } = 30;

        [Display(Name = "Mô tả nội dung buổi học")]
        public string? Description { get; set; }

        [Display(Name = "Trạng thái lớp")]
        public ClassStatus Status { get; set; } = ClassStatus.Upcoming;

        [Display(Name = "Lớp học công khai (Public)")]
        public bool IsPublic { get; set; } = false; // false = Private (chỉ học viên mua Course mới vào được), true = Public (ai cũng vào được)

        public virtual ICollection<LiveClassStudent> ClassStudents { get; set; } = new List<LiveClassStudent>();
    }

    public class LiveClassStudent
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LiveClassId { get; set; }

        [ForeignKey(nameof(LiveClassId))]
        public LiveClass? LiveClass { get; set; }

        [Required]
        public string StudentId { get; set; } = string.Empty;

        [ForeignKey(nameof(StudentId))]
        public ApplicationUser? Student { get; set; }

        public DateTime RegisteredAt { get; set; } = DateTime.Now;
    }
}

