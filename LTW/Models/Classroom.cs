using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LTW.Models
{
    public class ClassRoom
    {
        [Key]
        public int ClassRoomId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên lớp")]
        [StringLength(100)]
        [Display(Name = "Tên lớp")]
        public string Name { get; set; } = string.Empty;

        // Ví dụ: "Thứ 2, 4, 6 - 18:00 đến 19:30"
        [StringLength(200)]
        [Display(Name = "Lịch học")]
        public string? Schedule { get; set; }

        [Display(Name = "Giáo viên")]
        public int? TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public UserAccount? Teacher { get; set; }

        [InverseProperty(nameof(UserAccount.ClassRoom))]
        public ICollection<UserAccount> Students { get; set; } = new List<UserAccount>();

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}
