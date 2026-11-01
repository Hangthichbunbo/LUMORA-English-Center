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

            if (filter == "Public")
            {
                query = query.Where(c => c.IsPublic);
            }
            else if (filter == "MyClasses" && user != null)
            {
                if (isTeacher || isAdmin)
                {
                    query = query.Where(c => isAdmin || c.TeacherId == user.Id || (c.CourseId != null && enrolledCourseIds.Contains(c.CourseId.Value)));
                }
                else
                {
                    query = query.Where(c => c.IsPublic || (c.CourseId != null && enrolledCourseIds.Contains(c.CourseId.Value)));
                }
            }

            var classes = await query.OrderBy(c => c.ScheduledDate).ThenBy(c => c.StartTime).ToListAsync();

            ViewBag.CurrentFilter = filter ?? "All";
            ViewBag.EnrolledCourseIds = enrolledCourseIds;
            ViewBag.IsTeacher = isTeacher || isAdmin;
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
            bool isTeacherOrAdmin = false;

            if (liveClass.IsPublic)
            {
                canJoin = true;
            }

            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
                    bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";
                    bool isHostOrCourseTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);

                    isTeacherOrAdmin = isTeacher || isAdmin || isHostOrCourseTeacher;

                    if (isTeacherOrAdmin)
                    {
                        canJoin = true;
                    }
                    else if (!liveClass.IsPublic && liveClass.CourseId != null)
                    {
                        canJoin = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                    }
                }
            }

            ViewBag.CanJoin = canJoin;
            ViewBag.IsTeacher = isTeacherOrAdmin;
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
            bool isHostOrCourseTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);
            bool hasTeachingPrivilege = isTeacher || isAdmin || isHostOrCourseTeacher;

            // Business Rule: Check Private Class vs Public Class enrollment (Teachers/Admins have full access)
            if (!liveClass.IsPublic && liveClass.CourseId != null && !hasTeachingPrivilege)
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                if (!isEnrolled)
                {
                    TempData["ErrorMessage"] = $"Lớp học này là Private Live Class dành riêng cho học viên đã mua khóa học '{liveClass.Course?.Title}'. Vui lòng mua khóa học để tham gia!";
                    return RedirectToAction("Detail", "Course", new { id = liveClass.CourseId.Value });
                }
            }

            // Register student attendance only for regular students
            if (!hasTeachingPrivilege)
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
            bool isHostOrCourseTeacher = liveClass.TeacherId == user.Id || (liveClass.Course != null && liveClass.Course.TeacherId == user.Id);
            bool hasTeachingPrivilege = isTeacher || isAdmin || isHostOrCourseTeacher;

            // Strict check: Only students are checked for course purchase
            if (!liveClass.IsPublic && liveClass.CourseId != null && !hasTeachingPrivilege)
            {
                var isEnrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == liveClass.CourseId.Value && e.PaymentStatus == "Completed");
                if (!isEnrolled)
                {
                    TempData["ErrorMessage"] = "Bạn chưa đăng ký khóa học này.";
                    return RedirectToAction("Detail", "Course", new { id = liveClass.CourseId.Value });
                }
            }

            ViewBag.IsCurrentTeacher = hasTeachingPrivilege;
            return View(liveClass);
        }
    }
}

