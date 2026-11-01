using System.ComponentModel.DataAnnotations;
using StoryHub.Application.Common.Models;
using StoryHub.Domain.Entities;

namespace StoryHub.Web.Areas.Admin.ViewModels;

public class UserItemViewModel
{
    public ApplicationUser User { get; set; } = null!;
    public IList<string> Roles { get; set; } = [];
    public bool IsSeedAdmin { get; set; }
}

public class UserManagementIndexViewModel
{
    public PaginatedList<UserItemViewModel> Users { get; set; } = null!;
    public bool IsCurrentSeedAdmin { get; set; }
    public string SeedAdminEmail { get; set; } = string.Empty;
    public string? Search { get; set; }
}

public class CreateAdminUserViewModel
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên hiển thị là bắt buộc.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Tên hiển thị từ 2 đến 50 ký tự.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
