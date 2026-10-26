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

        private async Task PopulateTeacherPortalHeaderAsync(string activeTab)
        {
            ViewBag.ActiveTeacherTab = activeTab;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var teacherId = user.Id;
            var isAdmin = User.IsInRole("Admin");

            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == teacherId || isAdmin)
                .Select(c => c.CourseId)
                .ToListAsync();

            ViewBag.HeaderMyClassesCount = await _context.LiveClasses
                .Where(c => c.TeacherId == teacherId || isAdmin)
                .CountAsync();

            ViewBag.HeaderTotalStudents = await _context.Enrollments
                .Where(e => courseIds.Contains(e.CourseId) && e.PaymentStatus == "Completed")
                .Select(e => e.StudentId)
                .Distinct()
                .CountAsync();

            ViewBag.HeaderPendingAssessments = await _context.ExerciseSubmissions
                .Include(s => s.Exercise)
                    .ThenInclude(ex => ex!.Lesson)
                        .ThenInclude(l => l!.Chapter)
                .Where(s => courseIds.Contains(s.Exercise!.Lesson!.Chapter!.CourseId) && s.TeacherScore == null)
                .CountAsync();
        }

        private async Task<List<TeacherCourseOptionDto>> GetTeacherCoursesHierarchyAsync(string teacherId)
        {
            var isAdmin = User.IsInRole("Admin");
            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId || isAdmin)
                .OrderBy(c => c.Title)
                .Select(c => new TeacherCourseOptionDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Chapters = c.Chapters.OrderBy(ch => ch.OrderIndex).Select(ch => new TeacherChapterOptionDto
                    {
                        ChapterId = ch.ChapterId,
                        Title = ch.Title,
                        OrderIndex = ch.OrderIndex,
                        DisplayText = $"Chương {ch.OrderIndex}: {ch.Title}"
                    }).ToList()
                })
                .ToListAsync();

            return courses;
        }

        // GET: /Teacher/Dashboard
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var teacherId = user.Id;
            var isAdmin = User.IsInRole("Admin");

            var taughtCourses = await _context.Courses
                .Where(c => c.TeacherId == teacherId || isAdmin)
                .Include(c => c.Enrollments)
                .Include(c => c.Chapters)
                    .ThenInclude(ch => ch.Lessons)
                .ToListAsync();

            var courseIds = taughtCourses.Select(c => c.CourseId).ToList();

            var myClasses = await _context.LiveClasses
                .Where(c => c.TeacherId == teacherId || isAdmin)
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

            await PopulateTeacherPortalHeaderAsync("Dashboard");

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

            await PopulateTeacherPortalHeaderAsync("MyClasses");

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

            await PopulateTeacherPortalHeaderAsync("Lessons");

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

            var courses = await GetTeacherCoursesHierarchyAsync(user.Id);

            ViewBag.Courses = courses;
            ViewBag.CoursesJson = JsonSerializer.Serialize(courses);

            var chapters = courses.SelectMany(c => c.Chapters.Select(ch => new { ch.ChapterId, Title = $"{c.Title} - {ch.DisplayText}" })).ToList();
            ViewBag.Chapters = new SelectList(chapters, "ChapterId", "Title");

            return View(new Lesson { OrderIndex = 1 });
        }

        // POST: /Teacher/CreateLesson
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLesson(
            Lesson lesson, 
            int? courseId, 
            bool isNewChapter = false, 
            string? newChapterTitle = null, 
            int? newChapterOrder = null)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Clear model validation for navigation properties and optional/server-set fields
            ModelState.Remove("Chapter");
            ModelState.Remove("Exercises");
            ModelState.Remove("LessonProgresses");
            ModelState.Remove("Content");

            lesson.Content ??= string.Empty;

            if (isNewChapter)
            {
                ModelState.Remove("ChapterId");

                if (courseId == null || courseId <= 0)
                {
                    ModelState.AddModelError("courseId", "Vui lòng chọn khóa học để tạo chương mới");
                }
                else if (string.IsNullOrWhiteSpace(newChapterTitle))
                {
                    ModelState.AddModelError("newChapterTitle", "Vui lòng nhập tên chương mới");
                }
                else
                {
                    var course = await _context.Courses
                        .Include(c => c.Chapters)
                        .FirstOrDefaultAsync(c => c.CourseId == courseId.Value && (c.TeacherId == user.Id || User.IsInRole("Admin")));

                    if (course == null)
                    {
                        ModelState.AddModelError("courseId", "Khóa học không tồn tại hoặc bạn không có quyền phụ trách khóa này");
                    }
                    else
                    {
                        var maxOrder = course.Chapters.Any() ? course.Chapters.Max(ch => ch.OrderIndex) : 0;
                        var order = (newChapterOrder.HasValue && newChapterOrder.Value > 0) ? newChapterOrder.Value : (maxOrder + 1);

                        var createdChapter = new Chapter
                        {
                            CourseId = course.CourseId,
                            Title = newChapterTitle.Trim(),
                            OrderIndex = order
                        };
                        _context.Chapters.Add(createdChapter);
                        await _context.SaveChangesAsync();

                        lesson.ChapterId = createdChapter.ChapterId;
                        ModelState.Remove("ChapterId");
                    }
                }
            }
            else
            {
                if (lesson.ChapterId <= 0)
                {
                    ModelState.AddModelError("ChapterId", "Vui lòng chọn chương học");
                }
                else
                {
                    var chapter = await _context.Chapters
                        .Include(ch => ch.Course)
                        .FirstOrDefaultAsync(ch => ch.ChapterId == lesson.ChapterId && (ch.Course!.TeacherId == user.Id || User.IsInRole("Admin")));
                    if (chapter == null)
                    {
                        ModelState.AddModelError("ChapterId", "Chương học không tồn tại hoặc bạn không có quyền quản lý");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                if (lesson.OrderIndex <= 0)
                {
                    var maxLessonOrder = await _context.Lessons
                        .Where(l => l.ChapterId == lesson.ChapterId)
                        .Select(l => (int?)l.OrderIndex)
                        .MaxAsync() ?? 0;
                    lesson.OrderIndex = maxLessonOrder + 1;
                }

                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = isNewChapter
                    ? $"Đã tạo chương mới '{newChapterTitle}' và thêm bài học '{lesson.Title}' thành công!"
                    : $"Thêm bài học '{lesson.Title}' thành công!";
                return RedirectToAction(nameof(Lessons));
            }

            var courses = await GetTeacherCoursesHierarchyAsync(user.Id);
            ViewBag.Courses = courses;
            ViewBag.CoursesJson = JsonSerializer.Serialize(courses);

            var chapters = courses.SelectMany(c => c.Chapters.Select(ch => new { ch.ChapterId, Title = $"{c.Title} - {ch.DisplayText}" })).ToList();
            ViewBag.Chapters = new SelectList(chapters, "ChapterId", "Title");

            ViewBag.SelectedCourseId = courseId;
            ViewBag.IsNewChapter = isNewChapter;
            ViewBag.NewChapterTitle = newChapterTitle;
            ViewBag.NewChapterOrder = newChapterOrder;

            if (!isNewChapter && lesson.ChapterId > 0)
            {
                var matchedCourse = courses.FirstOrDefault(c => c.Chapters.Any(ch => ch.ChapterId == lesson.ChapterId));
                if (matchedCourse != null)
                {
                    ViewBag.SelectedCourseId = matchedCourse.CourseId;
                    ViewBag.SelectedChapterId = lesson.ChapterId;
                }
            }

            return View(lesson);
        }

        // GET: /Teacher/Exercises
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Exercises()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await PopulateTeacherPortalHeaderAsync("Exercises");

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
        public async Task<IActionResult> CreateExercise(int? lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var coursesHierarchy = await _context.Courses
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .OrderBy(c => c.Title)
                .Select(c => new {
                    c.CourseId,
                    c.Title,
                    Chapters = c.Chapters.OrderBy(ch => ch.OrderIndex).Select(ch => new {
                        ch.ChapterId,
                        ch.Title,
                        ch.OrderIndex,
                        DisplayText = $"Chương {ch.OrderIndex}: {ch.Title}",
                        Lessons = ch.Lessons.OrderBy(l => l.OrderIndex).Select(l => new {
                            l.LessonId,
                            l.Title,
                            l.OrderIndex,
                            DisplayText = $"Bài {l.OrderIndex}: {l.Title}"
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            ViewBag.CoursesHierarchy = coursesHierarchy;
            ViewBag.CoursesHierarchyJson = JsonSerializer.Serialize(coursesHierarchy);

            var flatLessons = coursesHierarchy
                .SelectMany(c => c.Chapters.SelectMany(ch => ch.Lessons.Select(l => new { l.LessonId, Title = $"{c.Title} > {ch.DisplayText} > {l.DisplayText}" })))
                .ToList();
            ViewBag.Lessons = new SelectList(flatLessons, "LessonId", "Title");

            if (lessonId.HasValue && lessonId.Value > 0)
            {
                foreach (var c in coursesHierarchy)
                {
                    foreach (var ch in c.Chapters)
                    {
                        if (ch.Lessons.Any(l => l.LessonId == lessonId.Value))
                        {
                            ViewBag.SelectedCourseId = c.CourseId;
                            ViewBag.SelectedChapterId = ch.ChapterId;
                            ViewBag.SelectedLessonId = lessonId.Value;
                            break;
                        }
                    }
                }
            }

            return View(new Exercise { LessonId = lessonId ?? 0, TimeLimitMinutes = 20, PassingScore = 75 });
        }

        // POST: /Teacher/CreateExercise
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExercise(
            Exercise exercise, 
            List<QuestionInputModel>? questions, 
            string? questionContent, 
            string? optA, 
            string? optB, 
            string? optC, 
            string? optD, 
            string? correctOpt, 
            string? explanation)
        {
            ModelState.Remove("Lesson");
            ModelState.Remove("Questions");
            ModelState.Remove("Submissions");

            if (exercise.LessonId <= 0)
            {
                ModelState.AddModelError("LessonId", "Vui lòng chọn bài học trực thuộc");
            }

            if (ModelState.IsValid)
            {
                exercise.CreatedAt = DateTime.Now;
                _context.Exercises.Add(exercise);
                await _context.SaveChangesAsync();

                int questionsAdded = 0;

                // 1. Process multiple questions if submitted
                if (questions != null && questions.Any())
                {
                    foreach (var qItem in questions)
                    {
                        if (string.IsNullOrWhiteSpace(qItem.Content)) continue;

                        var isWriting = string.Equals(qItem.Format, "Writing", StringComparison.OrdinalIgnoreCase);
                        var format = isWriting ? QuestionFormat.Writing : QuestionFormat.MultipleChoice;

                        string? optionsJson = null;
                        string correctAnswer = qItem.CorrectAnswer?.Trim() ?? "";

                        if (!isWriting)
                        {
                            var options = new[]
                            {
                                $"A. {qItem.OptA ?? ""}".Trim(),
                                $"B. {qItem.OptB ?? ""}".Trim(),
                                $"C. {qItem.OptC ?? ""}".Trim(),
                                $"D. {qItem.OptD ?? ""}".Trim()
                            };
                            optionsJson = JsonSerializer.Serialize(options);

                            // Normalize answer key
                            if (string.IsNullOrWhiteSpace(correctAnswer))
                            {
                                correctAnswer = options[0];
                            }
                            else if (correctAnswer.Length == 1)
                            {
                                switch (correctAnswer.ToUpperInvariant())
                                {
                                    case "A": correctAnswer = options[0]; break;
                                    case "B": correctAnswer = options[1]; break;
                                    case "C": correctAnswer = options[2]; break;
                                    case "D": correctAnswer = options[3]; break;
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(correctAnswer))
                            {
                                correctAnswer = "Tự luận - Giảng viên chấm điểm theo rubric";
                            }
                        }

                        var q = new ExerciseQuestion
                        {
                            ExerciseId = exercise.ExerciseId,
                            Skill = qItem.Skill != 0 ? qItem.Skill : SkillType.Reading,
                            Format = format,
                            Content = qItem.Content.Trim(),
                            OptionsJson = optionsJson,
                            CorrectAnswer = correctAnswer,
                            Explanation = qItem.Explanation,
                            Points = qItem.Points > 0 ? qItem.Points : 10
                        };
                        _context.ExerciseQuestions.Add(q);
                        questionsAdded++;
                    }
                }

                // 2. Fallback to single question parameters if questions list was empty
                if (questionsAdded == 0 && !string.IsNullOrWhiteSpace(questionContent))
                {
                    var options = new[] { $"A. {optA}", $"B. {optB}", $"C. {optC}", $"D. {optD}" };
                    var q = new ExerciseQuestion
                    {
                        ExerciseId = exercise.ExerciseId,
                        Skill = SkillType.Reading,
                        Format = QuestionFormat.MultipleChoice,
                        Content = questionContent.Trim(),
                        OptionsJson = JsonSerializer.Serialize(options),
                        CorrectAnswer = string.IsNullOrWhiteSpace(correctOpt) ? $"A. {optA}" : correctOpt.Trim(),
                        Explanation = explanation,
                        Points = 10
                    };
                    _context.ExerciseQuestions.Add(q);
                    questionsAdded++;
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Tạo bài tập và {questionsAdded} câu hỏi thành công!";
                return RedirectToAction(nameof(Exercises));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var coursesHierarchy = await _context.Courses
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .OrderBy(c => c.Title)
                .Select(c => new {
                    c.CourseId,
                    c.Title,
                    Chapters = c.Chapters.OrderBy(ch => ch.OrderIndex).Select(ch => new {
                        ch.ChapterId,
                        ch.Title,
                        ch.OrderIndex,
                        DisplayText = $"Chương {ch.OrderIndex}: {ch.Title}",
                        Lessons = ch.Lessons.OrderBy(l => l.OrderIndex).Select(l => new {
                            l.LessonId,
                            l.Title,
                            l.OrderIndex,
                            DisplayText = $"Bài {l.OrderIndex}: {l.Title}"
                        }).ToList()
                    }).ToList()
                })
                .ToListAsync();

            ViewBag.CoursesHierarchy = coursesHierarchy;
            ViewBag.CoursesHierarchyJson = JsonSerializer.Serialize(coursesHierarchy);

            var flatLessons = coursesHierarchy
                .SelectMany(c => c.Chapters.SelectMany(ch => ch.Lessons.Select(l => new { l.LessonId, Title = $"{c.Title} > {ch.DisplayText} > {l.DisplayText}" })))
                .ToList();
            ViewBag.Lessons = new SelectList(flatLessons, "LessonId", "Title");

            if (exercise.LessonId > 0)
            {
                foreach (var c in coursesHierarchy)
                {
                    foreach (var ch in c.Chapters)
                    {
                        if (ch.Lessons.Any(l => l.LessonId == exercise.LessonId))
                        {
                            ViewBag.SelectedCourseId = c.CourseId;
                            ViewBag.SelectedChapterId = ch.ChapterId;
                            ViewBag.SelectedLessonId = exercise.LessonId;
                            break;
                        }
                    }
                }
            }

            return View(exercise);
        }

        // GET: /Teacher/Students
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> Students()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            await PopulateTeacherPortalHeaderAsync("Students");

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

            await PopulateTeacherPortalHeaderAsync("Assessments");

            var courseIds = await _context.Courses
                .Where(c => c.TeacherId == user.Id || User.IsInRole("Admin"))
                .Select(c => c.CourseId)
                .ToListAsync();

            var submissions = await _context.ExerciseSubmissions
                .Include(s => s.Exercise)
                    .ThenInclude(ex => ex!.Questions)
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
            submission.TotalScore = submission.AutoScore + teacherScore;
            submission.TeacherComment = teacherComment;

            // Notify student
            var notif = new Notification
            {
                UserId = submission.StudentId,
                Title = "Giảng viên đã chấm bài tập của bạn",
                Content = $"Bài tập '{submission.Exercise?.Title}' của bạn đã được giảng viên chấm phần tự luận: {teacherScore:G29} điểm. Tổng điểm: {submission.TotalScore:G29}. Lời nhận xét: {teacherComment}",
                Type = "ExerciseDue",
                LinkUrl = "/Student/Exercise/" + submission.ExerciseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notif);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chấm điểm cho học viên {submission.Student?.FullName} thành công! Tổng điểm: {submission.TotalScore:G29} (Trắc nghiệm: {submission.AutoScore:G29} + Tự luận: {teacherScore:G29})";
            return RedirectToAction(nameof(Assessments));
        }

        // GET: /Teacher/GetChaptersByCourse?courseId=1
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetChaptersByCourse(int courseId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var chapters = await _context.Chapters
                .Where(ch => ch.CourseId == courseId && (ch.Course!.TeacherId == user.Id || User.IsInRole("Admin")))
                .OrderBy(ch => ch.OrderIndex)
                .Select(ch => new {
                    ch.ChapterId,
                    ch.Title,
                    ch.OrderIndex,
                    displayText = $"Chương {ch.OrderIndex}: {ch.Title}"
                })
                .ToListAsync();

            return Json(chapters);
        }

        // GET: /Teacher/GetLessonsByChapter?chapterId=1
        [HttpGet]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<IActionResult> GetLessonsByChapter(int chapterId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lessons = await _context.Lessons
                .Where(l => l.ChapterId == chapterId && (l.Chapter!.Course!.TeacherId == user.Id || User.IsInRole("Admin")))
                .OrderBy(l => l.OrderIndex)
                .Select(l => new {
                    l.LessonId,
                    l.Title,
                    l.OrderIndex,
                    displayText = $"Bài {l.OrderIndex}: {l.Title}"
                })
                .ToListAsync();

            return Json(lessons);
        }

        // POST: /Teacher/QuickCreateChapter
        [HttpPost]
        [Authorize(Roles = "Teacher,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickCreateChapter(int courseId, string title, int? orderIndex)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (courseId <= 0 || string.IsNullOrWhiteSpace(title))
            {
                return Json(new { success = false, message = "Vui lòng chọn khóa học và nhập tên chương học." });
            }

            var course = await _context.Courses
                .Include(c => c.Chapters)
                .FirstOrDefaultAsync(c => c.CourseId == courseId && (c.TeacherId == user.Id || User.IsInRole("Admin")));

            if (course == null)
            {
                return Json(new { success = false, message = "Khóa học không tồn tại hoặc bạn không có quyền phụ trách khóa này." });
            }

            var maxOrder = course.Chapters.Any() ? course.Chapters.Max(ch => ch.OrderIndex) : 0;
            var order = (orderIndex.HasValue && orderIndex.Value > 0) ? orderIndex.Value : (maxOrder + 1);

            var chapter = new Chapter
            {
                CourseId = course.CourseId,
                Title = title.Trim(),
                OrderIndex = order
            };

            _context.Chapters.Add(chapter);
            await _context.SaveChangesAsync();

            return Json(new {
                success = true,
                chapterId = chapter.ChapterId,
                title = chapter.Title,
                orderIndex = chapter.OrderIndex,
                displayText = $"Chương {chapter.OrderIndex}: {chapter.Title}",
                message = "Tạo chương mới thành công!"
            });
        }
    }
}

