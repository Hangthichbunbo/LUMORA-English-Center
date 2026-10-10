using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;

namespace LTW.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var totalStudents = await _userManager.GetUsersInRoleAsync("Student");
            var totalTeachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var totalCourses = await _context.Courses.CountAsync();
            var totalClasses = await _context.LiveClasses.CountAsync();
            var activeClasses = await _context.LiveClasses.CountAsync(c => c.Status != ClassStatus.Closed);
            var totalRevenue = await _context.Enrollments.Where(e => e.PaymentStatus == "Completed").SumAsync(e => e.PricePaid);

            var recentEnrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .Take(5)
                .ToListAsync();

            var recentUsers = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalStudents = totalStudents.Count;
            ViewBag.TotalTeachers = totalTeachers.Count;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.ActiveClasses = activeClasses;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.RecentEnrollments = recentEnrollments;
            ViewBag.RecentUsers = recentUsers;

            return View();
        }

        // GET: /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users(string? role, string? search)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role) && role != "All")
            {
                query = query.Where(u => u.RoleName.ToLower() == role.ToLower());
            }

            if (!string.IsNullOrEmpty(search))
            {
                var s = search.ToLower().Trim();
                query = query.Where(u => u.FullName.ToLower().Contains(s) || (u.Email != null && u.Email.ToLower().Contains(s)) || (u.UserName != null && u.UserName.ToLower().Contains(s)));
            }

            var users = await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
            ViewBag.CurrentRole = role ?? "All";
            ViewBag.CurrentSearch = search ?? "";

            return View(users);
        }

        // POST: /Admin/CreateTeacher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(string fullName, string email, string username, string password, string bio)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ các thông tin bắt buộc của giảng viên.";
                return RedirectToAction(nameof(Users));
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                TempData["ErrorMessage"] = "Email này đã được sử dụng trong hệ thống.";
                return RedirectToAction(nameof(Users));
            }

            var teacher = new ApplicationUser
            {
                UserName = username,
                Email = email,
                FullName = fullName,
                RoleName = "Teacher",
                EmailConfirmed = true,
                IsActive = true,
                Bio = bio,
                AvatarUrl = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?auto=format&fit=crop&w=200&q=80"
            };

            var result = await _userManager.CreateAsync(teacher, password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(teacher, "Teacher");
                TempData["SuccessMessage"] = $"Cấp tài khoản giảng viên cho '{fullName}' thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join("; ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/ToggleUserActive
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserActive(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                await _userManager.UpdateAsync(user);
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái hoạt động của tài khoản {user.UserName}.";
            }
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                if (user.UserName == "admin")
                {
                    TempData["ErrorMessage"] = "Không thể xóa tài khoản Quản trị viên mặc định!";
                    return RedirectToAction(nameof(Users));
                }

                await _userManager.DeleteAsync(user);
                TempData["SuccessMessage"] = $"Đã xóa tài khoản {user.UserName} khỏi hệ thống.";
            }
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Courses
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Include(c => c.Chapters)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Admin/CreateCourse
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            return View(new Course { Price = 1500000, OriginalPrice = 2000000, Duration = "10 tuần" });
        }

        // POST: /Admin/CreateCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Create initial default chapter
                var chapter = new Chapter
                {
                    CourseId = course.CourseId,
                    Title = "Chương 1: Nhập Môn & Tổng Quan Khóa Học",
                    OrderIndex = 1
                };
                _context.Chapters.Add(chapter);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo khóa học '{course.Title}' thành công!";
                return RedirectToAction(nameof(Courses));
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            return View(course);
        }

        // POST: /Admin/TogglePublishCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublishCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                course.IsPublished = !course.IsPublished;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã {(course.IsPublished ? "xuất bản" : "ẩn")} khóa học '{course.Title}'.";
            }
            return RedirectToAction(nameof(Courses));
        }

        // POST: /Admin/DeleteCourse
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa khóa học thành công.";
            }
            return RedirectToAction(nameof(Courses));
        }

        // GET: /Admin/EditCourse/5
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return NotFound();

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", course.TeacherId);
            return View(course);
        }

        // POST: /Admin/EditCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course)
        {
            if (id != course.CourseId) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.Courses.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Title = course.Title;
                existing.Description = course.Description;
                existing.Thumbnail = course.Thumbnail;
                existing.Price = course.Price;
                existing.OriginalPrice = course.OriginalPrice;
                existing.Duration = course.Duration;
                existing.Level = course.Level;
                existing.Category = course.Category;
                existing.TeacherId = course.TeacherId;
                existing.IsFeatured = course.IsFeatured;
                existing.IsPublished = course.IsPublished;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật khóa học '{course.Title}' thành công!";
                return RedirectToAction(nameof(Courses));
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", course.TeacherId);
            return View(course);
        }

        // GET: /Admin/Classes
        [HttpGet]
        public async Task<IActionResult> Classes()
        {
            var classes = await _context.LiveClasses
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .Include(c => c.ClassStudents)
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();

            return View(classes);
        }

        // GET: /Admin/CreateClass
        [HttpGet]
        public async Task<IActionResult> CreateClass()
        {
            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var courses = await _context.Courses.ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            ViewBag.Courses = new SelectList(courses, "CourseId", "Title");

            return View(new LiveClass 
            { 
                ScheduledDate = DateTime.Today.AddDays(2), 
                StartTime = new TimeSpan(19, 30, 0), 
                EndTime = new TimeSpan(21, 0, 0),
                Capacity = 30
            });
        }

        // POST: /Admin/CreateClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(LiveClass liveClass)
        {
            if (ModelState.IsValid)
            {
                _context.LiveClasses.Add(liveClass);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tạo lớp học trực tuyến '{liveClass.ClassName}' thành công!";
                return RedirectToAction(nameof(Classes));
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            ViewBag.Courses = new SelectList(courses, "CourseId", "Title");
            return View(liveClass);
        }

        // POST: /Admin/CloseClass
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseClass(int id)
        {
            var cls = await _context.LiveClasses.FindAsync(id);
            if (cls != null)
            {
                cls.Status = cls.Status == ClassStatus.Closed ? ClassStatus.Open : ClassStatus.Closed;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái lớp '{cls.ClassName}'.";
            }
            return RedirectToAction(nameof(Classes));
        }

        // GET: /Admin/EditClass/5
        [HttpGet]
        public async Task<IActionResult> EditClass(int id)
        {
            var cls = await _context.LiveClasses.FindAsync(id);
            if (cls == null) return NotFound();

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var courses = await _context.Courses.ToListAsync();

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", cls.TeacherId);
            ViewBag.Courses = new SelectList(courses, "CourseId", "Title", cls.CourseId);

            return View(cls);
        }

        // POST: /Admin/EditClass/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClass(int id, LiveClass liveClass)
        {
            if (id != liveClass.LiveClassId) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.LiveClasses.FindAsync(id);
                if (existing == null) return NotFound();

                existing.ClassName = liveClass.ClassName;
                existing.CourseId = liveClass.CourseId;
                existing.TeacherId = liveClass.TeacherId;
                existing.ScheduledDate = liveClass.ScheduledDate;
                existing.StartTime = liveClass.StartTime;
                existing.EndTime = liveClass.EndTime;
                existing.LiveRoomUrl = liveClass.LiveRoomUrl;
                existing.Capacity = liveClass.Capacity;
                existing.Description = liveClass.Description;
                existing.IsPublic = liveClass.IsPublic;
                existing.Status = liveClass.Status;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật lớp học '{liveClass.ClassName}' thành công!";
                return RedirectToAction(nameof(Classes));
            }

            var teachers = await _userManager.GetUsersInRoleAsync("Teacher");
            var courses = await _context.Courses.ToListAsync();
            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", liveClass.TeacherId);
            ViewBag.Courses = new SelectList(courses, "CourseId", "Title", liveClass.CourseId);
            return View(liveClass);
        }

        // GET: /Admin/Schedule
        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            var classes = await _context.LiveClasses
                .Include(c => c.Teacher)
                .Include(c => c.Course)
                .OrderBy(c => c.ScheduledDate)
                .ThenBy(c => c.StartTime)
                .ToListAsync();

            return View(classes);
        }

        // GET: /Admin/Tips
        [HttpGet]
        public async Task<IActionResult> Tips()
        {
            var tips = await _context.IeltsTips
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tips);
        }

        // GET: /Admin/CreateTip
        [HttpGet]
        public IActionResult CreateTip()
        {
            return View(new IeltsTip { AuthorName = "Ban Chuyên Môn Lumora" });
        }

        // POST: /Admin/CreateTip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTip(IeltsTip tip)
        {
            if (ModelState.IsValid)
            {
                tip.CreatedAt = DateTime.Now;
                tip.ViewsCount = 50;
                tip.IsPublished = true;
                _context.IeltsTips.Add(tip);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đăng bài viết IELTS Tip mới thành công!";
                return RedirectToAction(nameof(Tips));
            }
            return View(tip);
        }

        // POST: /Admin/TogglePublishTip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublishTip(int id)
        {
            var tip = await _context.IeltsTips.FindAsync(id);
            if (tip != null)
            {
                tip.IsPublished = !tip.IsPublished;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái bài viết '{tip.Title}'.";
            }
            return RedirectToAction(nameof(Tips));
        }

        // POST: /Admin/DeleteTip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTip(int id)
        {
            var tip = await _context.IeltsTips.FindAsync(id);
            if (tip != null)
            {
                _context.IeltsTips.Remove(tip);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa bài viết thành công.";
            }
            return RedirectToAction(nameof(Tips));
        }

        // GET: /Admin/EditTip/5
        [HttpGet]
        public async Task<IActionResult> EditTip(int id)
        {
            var tip = await _context.IeltsTips.FindAsync(id);
            if (tip == null) return NotFound();
            return View(tip);
        }

        // POST: /Admin/EditTip/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTip(int id, IeltsTip tip)
        {
            if (id != tip.TipId) return NotFound();

            if (ModelState.IsValid)
            {
                var existing = await _context.IeltsTips.FindAsync(id);
                if (existing == null) return NotFound();

                existing.Title = tip.Title;
                existing.Summary = tip.Summary;
                existing.Content = tip.Content;
                existing.Category = tip.Category;
                existing.Thumbnail = tip.Thumbnail;
                existing.AuthorName = tip.AuthorName;
                existing.IsPublished = tip.IsPublished;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật bài viết '{tip.Title}' thành công!";
                return RedirectToAction(nameof(Tips));
            }
            return View(tip);
        }

        // GET: /Admin/Tests
        [HttpGet]
        public async Task<IActionResult> Tests()
        {
            var tests = await _context.Tests
                .Include(t => t.Questions)
                .Include(t => t.Attempts)
                .OrderBy(t => t.Type)
                .ToListAsync();

            return View(tests);
        }

        // GET: /Admin/Notifications
        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var notifications = await _context.Notifications
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();

            return View(notifications);
        }

        // POST: /Admin/BroadcastNotification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastNotification(string title, string content, string role)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Tiêu đề và nội dung thông báo không được để trống.";
                return RedirectToAction(nameof(Notifications));
            }

            var query = _userManager.Users.AsQueryable();
            if (role != "All")
            {
                query = query.Where(u => u.RoleName == role);
            }

            var targetUsers = await query.ToListAsync();
            foreach (var u in targetUsers)
            {
                var n = new Notification
                {
                    UserId = u.Id,
                    Title = title,
                    Content = content,
                    Type = "System",
                    LinkUrl = "/Notification",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(n);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã phát sóng thông báo đến {targetUsers.Count} người dùng thành công!";
            return RedirectToAction(nameof(Notifications));
        }
    }
}

