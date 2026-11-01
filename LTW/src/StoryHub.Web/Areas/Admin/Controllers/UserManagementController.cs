using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StoryHub.Application.Common.Interfaces;
using StoryHub.Application.Common.Models;
using StoryHub.Application.Common.Options;
using StoryHub.Domain.Entities;
using StoryHub.Web.Areas.Admin.ViewModels;

namespace StoryHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAppDbContext? _dbContext;
    private readonly SeedSettings _seedSettings;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        IOptions<SeedSettings> seedOptions,
        IAppDbContext? dbContext = null,
        ILogger<UserManagementController>? logger = null)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _seedSettings = seedOptions.Value;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<UserManagementController>.Instance;
    }

    private async Task<bool> IsCurrentSeedAdminAsync()
    {
        var seedEmail = _seedSettings?.AdminEmail;
        if (string.IsNullOrWhiteSpace(seedEmail)) return false;

        if (string.Equals(User.Identity?.Name, seedEmail, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var currentUser = await _userManager.GetUserAsync(User);
        return currentUser != null && string.Equals(currentUser.Email, seedEmail, StringComparison.OrdinalIgnoreCase);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, int pageIndex = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(u => (u.Email != null && u.Email.Contains(term)) || (u.DisplayName != null && u.DisplayName.Contains(term)));
            }

            var paginatedUsers = await PaginatedList<ApplicationUser>.CreateAsync(
                query.OrderByDescending(u => u.CreatedAt),
                pageIndex,
                20,
                cancellationToken);

            var isCurrentSeedAdmin = await IsCurrentSeedAdminAsync();
            var seedEmail = _seedSettings?.AdminEmail ?? string.Empty;

            var items = new List<UserItemViewModel>();
            foreach (var u in paginatedUsers.Items)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var isSeed = !string.IsNullOrEmpty(seedEmail) && string.Equals(u.Email, seedEmail, StringComparison.OrdinalIgnoreCase);
                items.Add(new UserItemViewModel
                {
                    User = u,
                    Roles = roles,
                    IsSeedAdmin = isSeed
                });
            }

            var model = new UserManagementIndexViewModel
            {
                Users = new PaginatedList<UserItemViewModel>(items, paginatedUsers.TotalCount, paginatedUsers.PageIndex, 20),
                IsCurrentSeedAdmin = isCurrentSeedAdmin,
                SeedAdminEmail = seedEmail,
                Search = search
            };

            ViewData["Search"] = search;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi tải danh sách người dùng trong UserManagementController.Index");
            TempData["ErrorMessage"] = "Không thể tải danh sách người dùng do lỗi hệ thống.";
            var seedEmail = _seedSettings?.AdminEmail ?? string.Empty;
            return View(new UserManagementIndexViewModel
            {
                Users = new PaginatedList<UserItemViewModel>(new List<UserItemViewModel>(), 0, 1, 20),
                IsCurrentSeedAdmin = false,
                SeedAdminEmail = seedEmail,
                Search = search
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateAdminUserViewModel model)
    {
        if (!await IsCurrentSeedAdminAsync())
        {
            TempData["ErrorMessage"] = "Chỉ tài khoản Quản trị viên tối cao (từ SeedSettings) mới có quyền tạo tài khoản Admin.";
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Dữ liệu không hợp lệ: " + string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return RedirectToAction(nameof(Index));
        }

        var email = model.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            if (await _userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                TempData["ErrorMessage"] = $"Người dùng với email {email} đã tồn tại và đã có quyền Admin.";
            }
            else
            {
                await _userManager.AddToRoleAsync(existingUser, "Admin");
                TempData["SuccessMessage"] = $"Tài khoản {existingUser.DisplayName} ({email}) đã có sẵn trong hệ thống, vừa được cấp quyền Admin.";
            }
            return RedirectToAction(nameof(Index));
        }

        var newUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = model.DisplayName.Trim(),
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var createResult = await _userManager.CreateAsync(newUser, model.Password);
        if (!createResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Không thể tạo tài khoản: " + string.Join(", ", createResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(Index));
        }

        await _userManager.AddToRoleAsync(newUser, "Admin");
        TempData["SuccessMessage"] = $"Đã tạo mới và cấp quyền Quản trị viên (Admin) cho {newUser.DisplayName} ({newUser.Email}) thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GrantAdmin(string userId)
    {
        if (!await IsCurrentSeedAdminAsync())
        {
            TempData["ErrorMessage"] = "Chỉ tài khoản Quản trị viên tối cao (từ SeedSettings) mới có quyền cấp tài khoản Admin.";
            return Forbid();
        }

        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null) return NotFound();

        if (await _userManager.IsInRoleAsync(targetUser, "Admin"))
        {
            TempData["ErrorMessage"] = $"Người dùng {targetUser.DisplayName} đã có quyền Admin từ trước.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.AddToRoleAsync(targetUser, "Admin");
        TempData["SuccessMessage"] = $"Đã cấp quyền Quản trị viên (Admin) cho {targetUser.DisplayName} ({targetUser.Email}) thành công.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeAdmin(string userId)
    {
        if (!await IsCurrentSeedAdminAsync())
        {
            TempData["ErrorMessage"] = "Chỉ tài khoản Quản trị viên tối cao (từ SeedSettings) mới có quyền thu hồi quyền Admin.";
            return Forbid();
        }

        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null) return NotFound();

        if (string.Equals(targetUser.Email, _seedSettings.AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Không thể tước quyền Quản trị viên của tài khoản Quản trị viên tối cao (SeedSetting).";
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (targetUser.Id == currentUserId)
        {
            TempData["ErrorMessage"] = "Bạn không thể tự tước quyền Quản trị viên của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        if (await _userManager.IsInRoleAsync(targetUser, "Admin"))
        {
            await _userManager.RemoveFromRoleAsync(targetUser, "Admin");
            var remainingRoles = await _userManager.GetRolesAsync(targetUser);
            if (!remainingRoles.Any())
            {
                await _userManager.AddToRoleAsync(targetUser, "User");
            }
            TempData["SuccessMessage"] = $"Đã thu hồi quyền Admin của {targetUser.DisplayName} ({targetUser.Email}).";
        }
        else
        {
            TempData["ErrorMessage"] = $"Người dùng {targetUser.DisplayName} không có vai trò Admin.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAdmin(string userId)
    {
        if (!await IsCurrentSeedAdminAsync())
        {
            TempData["ErrorMessage"] = "Chỉ tài khoản Quản trị viên tối cao (từ SeedSettings) mới có quyền xóa tài khoản Admin.";
            return Forbid();
        }

        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null) return NotFound();

        if (string.Equals(targetUser.Email, _seedSettings.AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên tối cao (SeedSetting).";
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (targetUser.Id == currentUserId)
        {
            TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        var displayName = targetUser.DisplayName;
        var email = targetUser.Email;

        try
        {
            await CleanupUserDataAsync(targetUser.Id);

            var result = await _userManager.DeleteAsync(targetUser);
            if (result.Succeeded)
            {
                _logger.LogInformation("SuperAdmin {Admin} đã xóa tài khoản Admin {Name} ({Email}).", User.Identity?.Name, displayName, email);
                TempData["SuccessMessage"] = $"Đã xóa tài khoản Admin {displayName} ({email}) khỏi hệ thống.";
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa tài khoản Admin: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa tài khoản Admin {Email}", email);
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa tài khoản Admin.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            TempData["ErrorMessage"] = "Không tìm thấy mã định danh người dùng.";
            return RedirectToAction(nameof(Index));
        }

        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null) return NotFound();

        // 1. Không cho phép xóa Quản trị viên tối cao
        if (string.Equals(targetUser.Email, _seedSettings.AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên tối cao (SeedSetting).";
            return RedirectToAction(nameof(Index));
        }

        // 2. Không cho phép tự xóa chính mình
        var currentUserId = _userManager.GetUserId(User);
        if (targetUser.Id == currentUserId)
        {
            TempData["ErrorMessage"] = "Bạn không thể tự xóa tài khoản của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Nếu là Admin, yêu cầu quyền SuperAdmin qua nút Xóa Admin
        var isTargetAdmin = await _userManager.IsInRoleAsync(targetUser, "Admin");
        if (isTargetAdmin)
        {
            TempData["ErrorMessage"] = "Tài khoản này là Quản trị viên. Vui lòng sử dụng tính năng Xóa Admin của Quản trị viên tối cao.";
            return RedirectToAction(nameof(Index));
        }

        var displayName = targetUser.DisplayName;
        var email = targetUser.Email;

        try
        {
            await CleanupUserDataAsync(targetUser.Id);

            var result = await _userManager.DeleteAsync(targetUser);
            if (result.Succeeded)
            {
                _logger.LogInformation("Admin {AdminUser} đã xóa vĩnh viễn tài khoản người dùng {User} ({Email}).", User.Identity?.Name, displayName, email);
                TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn tài khoản người dùng {displayName} ({email}) và dữ liệu liên quan thành công.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không thể xóa tài khoản: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi xảy ra khi xóa người dùng {Email}", email);
            TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa người dùng.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CleanupUserDataAsync(string userId)
    {
        if (_dbContext == null) return;

        var userComments = await _dbContext.Comments.Where(c => c.UserId == userId).ToListAsync();
        if (userComments.Count != 0)
        {
            _dbContext.Comments.RemoveRange(userComments);
        }

        var userRatings = await _dbContext.Ratings.Where(r => r.UserId == userId).ToListAsync();
        if (userRatings.Count != 0)
        {
            _dbContext.Ratings.RemoveRange(userRatings);
        }

        var userReports = await _dbContext.CommentReports
            .Where(cr => cr.ReportedByUserId == userId || cr.ReviewedByUserId == userId)
            .ToListAsync();
        if (userReports.Count != 0)
        {
            _dbContext.CommentReports.RemoveRange(userReports);
        }

        var userFollows = await _dbContext.FollowedStories.Where(fs => fs.UserId == userId).ToListAsync();
        if (userFollows.Count != 0)
        {
            _dbContext.FollowedStories.RemoveRange(userFollows);
        }

        var userHistories = await _dbContext.ReadingHistories.Where(rh => rh.UserId == userId).ToListAsync();
        if (userHistories.Count != 0)
        {
            _dbContext.ReadingHistories.RemoveRange(userHistories);
        }

        await _dbContext.SaveChangesAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // Cannot deactivate yourself or the seed admin!
        if (user.UserName == User.Identity?.Name || user.Email == User.Identity?.Name)
        {
            TempData["ErrorMessage"] = "Bạn không thể tự vô hiệu hóa tài khoản của chính mình.";
            return RedirectToAction(nameof(Index));
        }

        if (string.Equals(user.Email, _seedSettings.AdminEmail, StringComparison.OrdinalIgnoreCase))
        {
            TempData["ErrorMessage"] = "Không thể vô hiệu hóa tài khoản Quản trị viên tối cao (SeedSetting).";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = $"Đã {(user.IsActive ? "kích hoạt" : "khóa")} tài khoản {user.DisplayName}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BlockComment(string userId, int days)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.CommentBlockedUntil = days > 0 ? DateTimeOffset.UtcNow.AddDays(days) : DateTimeOffset.UtcNow.AddYears(100);
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _userManager.UpdateAsync(user);
        TempData["SuccessMessage"] = $"Đã khóa quyền bình luận của {user.DisplayName} trong {days} ngày.";

        return RedirectToAction(nameof(Index));
    }
}
