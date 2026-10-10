using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;

namespace LTW.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notification/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // POST: /Notification/MarkAsRead/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == user.Id);
            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var unread = await _context.Notifications.Where(n => n.UserId == user.Id && !n.IsRead).ToListAsync();
            foreach (var n in unread)
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã đánh dấu tất cả thông báo là đã đọc.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Notification/SendEmailReminder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailReminder(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.NotificationId == id && n.UserId == user.Id);
            if (notif == null) return NotFound();

            // Simulation of email reminder delivery
            TempData["SuccessMessage"] = $"[Email Reminder]: Đã gửi thư điện tử nhắc nhở về tiêu đề '{notif.Title}' tới địa chỉ email {user.Email}!";
            return RedirectToAction(nameof(Index));
        }
    }
}

