using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;

namespace LTW.Controllers
{
    public class CourseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CourseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Course/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? category, string? search, string? level)
        {
            var query = _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                        .ThenInclude(l => l.Exercises)
                .Include(c => c.LiveClasses)
                .Where(c => c.IsPublished);

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                query = query.Where(c => c.Category.ToLower() == category.ToLower());
            }

            if (!string.IsNullOrEmpty(level) && level != "All")
            {
                query = query.Where(c => c.Level.ToLower().Contains(level.ToLower()));
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(c => c.Title.ToLower().Contains(s) || c.Description.ToLower().Contains(s));
            }

            var courses = await query.OrderByDescending(c => c.IsFeatured).ThenByDescending(c => c.CreatedAt).ToListAsync();

            ViewBag.CurrentCategory = category ?? "All";
            ViewBag.CurrentLevel = level ?? "All";
            ViewBag.CurrentSearch = search ?? "";

            return View(courses);
        }

        // GET: /Course/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Chapters.OrderBy(ch => ch.OrderIndex))
                    .ThenInclude(ch => ch.Lessons.OrderBy(l => l.OrderIndex))
                        .ThenInclude(l => l.Exercises)
                .Include(c => c.LiveClasses.OrderBy(lc => lc.ScheduledDate))
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            bool isEnrolled = false;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == id);
                }
            }

            ViewBag.IsEnrolled = isEnrolled;
            return View(course);
        }

        // GET: /Course/Purchase/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Purchase(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == id);
            if (alreadyEnrolled)
            {
                TempData["SuccessMessage"] = "Bạn đã đăng ký và sở hữu khóa học này rồi!";
                return RedirectToAction("Detail", new { id });
            }

            return View(course);
        }

        // POST: /Course/ProcessPayment
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(int courseId, string paymentMethod = "VNPay")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            var alreadyEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == courseId);
            if (alreadyEnrolled)
            {
                TempData["SuccessMessage"] = "Bạn đã sở hữu khóa học này!";
                return RedirectToAction("MyCourses", "Student");
            }

            var enrollment = new Enrollment
            {
                StudentId = user.Id,
                CourseId = courseId,
                EnrolledAt = DateTime.Now,
                PricePaid = course.Price,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Completed",
                TransactionId = "LUMORA-" + DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            _context.Enrollments.Add(enrollment);

            // Add notification to student
            var notification = new Notification
            {
                UserId = user.Id,
                Title = "Đăng ký khóa học thành công!",
                Content = $"Chúc mừng bạn đã sở hữu thành công khóa học '{course.Title}'. Bắt đầu bài học đầu tiên ngay hôm nay!",
                Type = "CourseUpdate",
                LinkUrl = "/Course/Detail/" + course.CourseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(PaymentSuccess), new { id = enrollment.EnrollmentId });
        }

        // GET: /Course/PaymentSuccess/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> PaymentSuccess(int id)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Teacher)
                .FirstOrDefaultAsync(e => e.EnrollmentId == id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return View(enrollment);
        }
    }
}

