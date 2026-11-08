using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;

namespace LTW.Controllers
{
    public class ClassController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClassController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Class/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? filter = "All")
        {
            var user = User.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(User) : null;
            var enrolledCourseIds = new List<int>();
            bool isTeacher = false;
            bool isAdmin = false;

            if (user != null)
            {
                isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
                isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";

                if (isTeacher || isAdmin)
                {
                    // Teachers and Admins have access to all courses they teach or all courses in general
                    enrolledCourseIds = await _context.Courses
                        .Where(c => isAdmin || c.TeacherId == user.Id)
                        .Select(c => c.CourseId)
                        .ToListAsync();
                }
                else
                {
                    enrolledCourseIds = await _context.Enrollments
                        .Where(e => e.StudentId == user.Id && e.PaymentStatus == "Completed")
                        .Select(e => e.CourseId)
                        .ToListAsync();
                }
            }

            var query = _context.LiveClasses
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .AsQueryable();

            // Non-admin / non-teacher users do not see closed classes
            if (!isAdmin && !isTeacher)
            {
                query = query.Where(c => c.Status != ClassStatus.Closed);
            }

            if (filter == "Public")
            {
                query = query.Where(c => c.IsPublic);
            }
            else if (filter == "MyClasses" && user != null)
            {
                if (isAdmin)
                {
                    query = query.Where(c => c.TeacherId == user.Id);
                }
                else if (isTeacher)
                {
                    query = query.Where(c => c.TeacherId == user.Id || (c.Course != null && c.Course.TeacherId == user.Id));
                }
                else
                {
                    query = query.Where(c => c.CourseId != null && enrolledCourseIds.Contains(c.CourseId.Value));
                }
            }

            var classes = await query.OrderBy(c => c.ScheduledDate).ThenBy(c => c.StartTime).ToListAsync();

            ViewBag.CurrentFilter = filter ?? "All";
            ViewBag.EnrolledCourseIds = enrolledCourseIds;
            ViewBag.IsTeacher = isTeacher;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.CurrentUserId = user?.Id;

            return View(classes);
        }

        // GET: /Class/Detail/5
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var liveClass = await _context.LiveClasses
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .Include(c => c.ClassStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(c => c.LiveClassId == id);

            if (liveClass == null) return NotFound();

            bool canJoin = false;
            bool isTeacher = false;
            bool isAdmin = false;
            bool isHostTeacher = false;

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
                    isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";
                    isHostTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);

                    if (isAdmin || isHostTeacher)
                    {
                        canJoin = true;
                    }
                    else if (liveClass.IsPublic)
                    {
                        canJoin = (liveClass.Status != ClassStatus.Closed);
                    }
                    else if (liveClass.CourseId != null)
                    {
                        var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                        canJoin = isEnrolled && (liveClass.Status != ClassStatus.Closed);
                    }
                }
            }
            else
            {
                canJoin = liveClass.IsPublic && (liveClass.Status != ClassStatus.Closed);
            }

            ViewBag.CanJoin = canJoin;
            ViewBag.IsTeacher = isTeacher;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsHostTeacher = isHostTeacher;
            return View(liveClass);
        }

        // GET: /Class/Join/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Join(int id)
        {
            var liveClass = await _context.LiveClasses
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.LiveClassId == id);

            if (liveClass == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";
            bool isHostTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);

            // Business Rule: Check if class is closed
            if (liveClass.Status == ClassStatus.Closed && !isAdmin)
            {
                TempData["ErrorMessage"] = "Lớp học này đã kết thúc hoặc tạm đóng bởi Quản trị viên.";
                return RedirectToAction(nameof(Index));
            }

            // Business Rule: Check Private Class vs Public Class enrollment
            // Admin and the host teacher of this class can always enter. Other teachers or students must be enrolled in the course if it's private.
            if (!liveClass.IsPublic && liveClass.CourseId != null && !isAdmin && !isHostTeacher)
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                if (!isEnrolled)
                {
                    TempData["ErrorMessage"] = $"Lớp học này là Private Live Class dành riêng cho học viên đã mua khóa học '{liveClass.Course?.Title}'. Vui lòng mua khóa học để tham gia!";
                    return RedirectToAction("Detail", "Course", new { id = liveClass.CourseId.Value });
                }
            }

            // Register attendance for any participant joining the room (guest teacher, admin observer, student) except the host teacher
            if (!isHostTeacher)
            {
                var existingAttendance = await _context.LiveClassStudents
                    .FirstOrDefaultAsync(cs => cs.LiveClassId == id && cs.StudentId == user.Id);

                if (existingAttendance == null)
                {
                    var attendance = new LiveClassStudent
                    {
                        LiveClassId = id,
                        StudentId = user.Id,
                        RegisteredAt = DateTime.Now
                    };
                    _context.LiveClassStudents.Add(attendance);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Room), new { id });
        }

        // GET: /Class/Room/5
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Room(int id)
        {
            var liveClass = await _context.LiveClasses
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .Include(c => c.ClassStudents)
                    .ThenInclude(cs => cs.Student)
                .FirstOrDefaultAsync(c => c.LiveClassId == id);

            if (liveClass == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";
            bool isHostTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);

            if (liveClass.Status == ClassStatus.Closed && !isAdmin)
            {
                TempData["ErrorMessage"] = "Lớp học này đã kết thúc hoặc tạm đóng bởi Quản trị viên.";
                return RedirectToAction(nameof(Index));
            }

            if (!liveClass.IsPublic && liveClass.CourseId != null && !isAdmin && !isHostTeacher)
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                if (!isEnrolled)
                {
                    TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này.";
                    return RedirectToAction("Detail", "Course", new { id = liveClass.CourseId.Value });
                }
            }

            ViewBag.CurrentUser = user;
            ViewBag.IsHostTeacher = isHostTeacher;
            ViewBag.IsTeacher = isTeacher;
            ViewBag.IsAdmin = isAdmin;
            return View(liveClass);
        }
    }
}

