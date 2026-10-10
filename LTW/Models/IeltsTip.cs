using System.ComponentModel.DataAnnotations;

namespace LTW.Models
{
    public class IeltsTip
    {
        [Key]
        public int TipId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [StringLength(250)]
        [Display(Name = "Tiêu đề bài viết")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tóm tắt")]
        [StringLength(500)]
        [Display(Name = "Tóm tắt ngắn")]
        public string Summary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập nội dung chi tiết")]
        [Display(Name = "Nội dung bài viết")]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ảnh đại diện")]
        public string Thumbnail { get; set; } = "/images/tips/default-tip.jpg";

        [Required]
        [StringLength(50)]
        [Display(Name = "Chuyên mục")]
        public string Category { get; set; } = "Reading"; // Listening, Reading, Writing, Speaking, Vocabulary, Grammar

        [StringLength(100)]
        [Display(Name = "Tác giả")]
        public string AuthorName { get; set; } = "Lumora Academic Team";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Lượt xem")]
        public int ViewsCount { get; set; } = 120;

        [Display(Name = "Xuất bản")]
        public bool IsPublished { get; set; } = true;
    }
}

