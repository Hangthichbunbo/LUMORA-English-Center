using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;
using System.Text.Json;

namespace LTW.Controllers
{
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TeacherController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================================
        // PUBLIC ACTIONS (Candidates & Visitors)
        // ==========================================

        // GET: /Teacher or /Teacher/Index
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var teachers = await _context.Users
                .Where(u => u.RoleName == "Teacher" && u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();

            var teacherIds = teachers.Select(t => t.Id).ToList();
            var courses = await _context.Courses
                .Where(c => c.TeacherId != null && teacherIds.Contains(c.TeacherId) && c.IsPublished)
                .ToListAsync();

            ViewBag.TeacherCourses = courses;
            return View(teachers);
        }

        // GET: /Teacher/Detail/{id}
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index));
            }

            var teacher = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.RoleName == "Teacher");

            if (teacher == null)
            {
                // Fallback lookup by UserName or Email
                teacher = await _context.Users
                    .FirstOrDefaultAsync(u => (u.UserName == id || u.Email == id) && u.RoleName == "Teacher");
            }

            if (teacher == null)
            {
                return NotFound();
            }

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id && c.IsPublished)
                .ToListAsync();

            var upcomingClasses = await _context.LiveClasses
                .Where(c => c.TeacherId == teacher.Id && c.ScheduledDate >= DateTime.Today)
                .OrderBy(c => c.ScheduledDate)
                .Take(4)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.UpcomingClasses = upcomingClasses;

            return View(teacher);
        }

        // ==========================================
        // TEACHER PORTAL & MANAGEMENT ACTIONS
        // ==========================================

        // GET: /Teacher/Dashboard
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var teacherId = user.Id;

            var taughtCourses = await _context.Courses
                .Where(c => c.TeacherId == teacherId || User.IsInRole("Admin"))
                .Include(c => c.Enrollments)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                .ToListAsync();

            var courseIds = taughtCourses.Select(c => c.CourseId).ToList();

            var myClasses = await _context.LiveClasses
                .Where(c => c.TeacherId == teacherId || User.IsInRole("Admin"))
                .OrderBy(c => c.ScheduledDate)
                .ToListAsync();

            var totalStudents = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.PaymentStatus == "Completed")
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            var pendingAssessments = await _context.ExerciseSubmissions
                .Include(s => s.Exercise)
                    .ThenInclude(ex => ex!.Lesson)
                        .ThenInclude(l => l!.Chapter)
                .Include(s => s.Student)
                .Where(s => courseIds.Contains(s.Exercise!.Lesson!.Chapter!.CourseId) && s.TeacherScore == null)
                .OrderByDescending(s => s.SubmittedAt)
                .Take(5)
                .ToListAsync();

            ViewBag.TaughtCourses = taughtCourses;
            ViewBag.MyClasses = myClasses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.PendingAssessments = pendingAssessments;

            return View();
        }

        // GET: /Teacher/MyClasses
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> MyClasses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var classes = await _context.LiveClasses
                .Include(c => c.Course)
                .Include(c => c.ClassStudents)
                    .ThenInclude(cs => cs.Student)
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .OrderByDescending(c => c.ScheduledDate)
                .ToListAsync();

            return View(classes);
        }

        // GET: /Teacher/Lessons
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Lessons()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lessons = await _context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch!.Course)
                .Include(l => l.Exercises)
                .Where(l => l.Chapter!.Course!.TeacherId == user.Id || User.IsInRole("Admin"))
                .OrderBy(l => l.Chapter!.CourseId)
                .ThenBy(l => l.Chapter!.OrderIndex)
                .ThenBy(l => l.OrderIndex)
                .ToListAsync();

            return View(lessons);
        }

        // GET: /Teacher/CreateLesson
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateLesson()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var chapters = await _context.Chapters
                .Include(ch => ch.Course)
                .Where(ch => ch.Course!.TeacherId == user.Id || User.IsInRole("Admin"))
                .Select(ch => new { ch.ChapterId, Title = $"{ch.Course!.Title} - Chương {ch.OrderIndex}: {ch.Title}" })
                .ToListAsync();

            ViewBag.Chapters = new SelectList(chapters, "ChapterId", "Title");
            return View(new Lesson { OrderIndex = 1 });
        }

        // POST: /Teacher/CreateLesson
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(Lesson lesson)
        {
            if (ModelState.IsValid)
            {
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tạo bài học mới thành công!";
                return RedirectToAction(nameof(Lessons));
            }

            var user = await _userManager.GetUserAsync(User);
            var chapters = await _context.Chapters
                .Include(ch => ch.Course)
                .Where(ch => ch.Course!.TeacherId == user!.Id || User.IsInRole("Admin"))
                .Select(ch => new { ch.ChapterId, Title = $"{ch.Course!.Title} - Chương {ch.OrderIndex}: {ch.Title}" })
                .ToListAsync();
            ViewBag.Chapters = new SelectList(chapters, "ChapterId", "Title");
            return View(lesson);
        }

        // GET: /Teacher/Exercises
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Exercises()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var exercises = await _context.Exercises
                .Include(e => e.Lesson)
                    .ThenInclude(l => l!.Chapter)
                        .ThenInclude(ch => ch!.Course)
                .Include(e => e.Questions)
                .Include(e => e.Submissions)
                .Where(e => e.Lesson!.Chapter!.Course!.TeacherId == user.Id || User.IsInRole("Admin"))
                .ToListAsync();

            return View(exercises);
        }

        // GET: /Teacher/CreateExercise
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> CreateExercise()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lessons = await _context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch!.Course)
                .Where(l => l.Chapter!.Course!.TeacherId == user.Id || User.IsInRole("Admin"))
                .Select(l => new { l.LessonId, Title = $"{l.Chapter!.Course!.Title} > {l.Title}" })
                .ToListAsync();

            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title");
            return View(new Exercise { TimeLimitMinutes = 20, PassingScore = 75 });
        }

        // POST: /Teacher/CreateExercise
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(Exercise exercise, string questionContent, string optA, string optB, string optC, string optD, string correctOpt, string explanation)
        {
            if (ModelState.IsValid)
            {
                exercise.CreatedAt = DateTime.Now;
                _context.Exercises.Add(exercise);
                await _context.SaveChangesAsync();

                // Add initial question
                if (!string.IsNullOrWhiteSpace(questionContent) && !string.IsNullOrWhiteSpace(correctOpt))
                {
                    var options = new[] { $"A. {optA}", $"B. {optB}", $"C. {optC}", $"D. {optD}" };
                    var q = new ExerciseQuestion
                    {
                        ExerciseId = exercise.ExerciseId,
                        Skill = SkillType.Reading,
                        Format = QuestionFormat.MultipleChoice,
                        Content = questionContent,
                        OptionsJson = JsonSerializer.Serialize(options),
                        CorrectAnswer = correctOpt,
                        Explanation = explanation,
                        Points = 10
                    };
                    _context.ExerciseQuestions.Add(q);
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Tạo bài tập và bộ câu hỏi thành công!";
                return RedirectToAction(nameof(Exercises));
            }

            var user = await _userManager.GetUserAsync(User);
            var lessons = await _context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch!.Course)
                .Where(l => l.Chapter!.Course!.TeacherId == user!.Id || User.IsInRole("Admin"))
                .Select(l => new { l.LessonId, Title = $"{l.Chapter!.Course!.Title} > {l.Title}" })
                .ToListAsync();
            ViewBag.Lessons = new SelectList(lessons, "LessonId", "Title");
            return View(exercise);
        }

        // GET: /Teacher/Students
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Students()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .Select(c => c.CourseId)
                .ToListAsync();

            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => courseIds.Contains(e.CourseId) && e.PaymentStatus == "Completed")
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var studentIds = enrollments.Select(e => e.StudentId).Distinct().ToList();
            var submissions = await _context.ExerciseSubmissions
                .Where(s => studentIds.Contains(s.StudentId))
                .ToListAsync();

            ViewBag.Submissions = submissions;
            return View(enrollments);
        }

        // GET: /Teacher/Assessments
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Assessments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .Select(c => c.CourseId)
                .ToListAsync();

            var submissions = await _context.ExerciseSubmissions
                .Include(s => s.Exercise)
                    .ThenInclude(ex => ex!.Lesson)
                        .ThenInclude(l => l!.Chapter)
                            .ThenInclude(ch => ch!.Course)
                .Include(s => s.Student)
                .Where(s => courseIds.Contains(s.Exercise!.Lesson!.Chapter!.CourseId))
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            return View(submissions);
        }

        // POST: /Teacher/GradeSubmission
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(int submissionId, decimal teacherScore, string teacherComment)
        {
            var submission = await _context.ExerciseSubmissions
                .Include(s => s.Exercise)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.SubmissionId == submissionId);

            if (submission == null) return NotFound();

            submission.TeacherScore = teacherScore;
            submission.TotalScore = Math.Round((submission.AutoScore + teacherScore) / 2.0m, 1);
            submission.TeacherComment = teacherComment;

            // Notify student
            var notif = new Notification
            {
                UserId = submission.StudentId,
                Title = "Giảng viên đã chấm bài tập của bạn",
                Content = $"Bài tập '{submission.Exercise?.Title}' của bạn đã được giảng viên chấm: {teacherScore:F1}/10. Lời nhận xét: {teacherComment}",
                Type = "ExerciseDue",
                LinkUrl = "/Student/Exercise/" + submission.ExerciseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notif);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chấm điểm cho học viên {submission.Student?.FullName} thành công!";
            return RedirectToAction(nameof(Assessments));
        }
    }
}

