using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;
using System.Text.Json;

namespace LTW.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Student/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 1. My Enrolled Courses & Progress
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Chapters)
                        .ThenInclude(ch => ch.Lessons)
                            .ThenInclude(l => l.Exercises)
                .Where(e => e.StudentId == user.Id && e.PaymentStatus == "Completed")
                .ToListAsync();

            var enrolledCourseIds = enrollments.Select(e => e.CourseId).ToList();

            // 2. Lesson Progress
            var completedLessonIds = await _context.LessonProgresses
                .Where(lp => lp.StudentId == user.Id && lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToListAsync();

            // 3. Upcoming Live Classes
            var upcomingClasses = await _context.LiveClasses
                .Include(lc => lc.Teacher)
                .Include(lc => lc.Course)
                .Where(lc => lc.ScheduledDate >= DateTime.Today && (lc.IsPublic || (lc.CourseId != null && enrolledCourseIds.Contains(lc.CourseId.Value))))
                .OrderBy(lc => lc.ScheduledDate)
                .ThenBy(lc => lc.StartTime)
                .Take(3)
                .ToListAsync();

            // 4. Pending Exercises
            var submittedExerciseIds = await _context.ExerciseSubmissions
                .Where(es => es.StudentId == user.Id)
                .Select(es => es.ExerciseId)
                .ToListAsync();

            var pendingExercises = await _context.Exercises
                .Include(ex => ex.Lesson)
                    .ThenInclude(l => l!.Chapter)
                        .ThenInclude(ch => ch!.Course)
                .Where(ex => enrolledCourseIds.Contains(ex.Lesson!.Chapter!.CourseId) && !submittedExerciseIds.Contains(ex.ExerciseId))
                .Take(4)
                .ToListAsync();

            // 5. Recent Submissions & Scores
            var recentSubmissions = await _context.ExerciseSubmissions
                .Include(es => es.Exercise)
                    .ThenInclude(ex => ex!.Questions)
                .Include(es => es.Exercise)
                    .ThenInclude(ex => ex!.Lesson)
                        .ThenInclude(l => l!.Chapter)
                            .ThenInclude(ch => ch!.Course)
                .Where(es => es.StudentId == user.Id)
                .OrderByDescending(es => es.SubmittedAt)
                .Take(6)
                .ToListAsync();

            // 6. Placement Test Result
            var latestTestAttempt = await _context.TestAttempts
                .Include(ta => ta.Test)
                .Where(ta => ta.StudentId == user.Id || ta.Email == user.Email)
                .OrderByDescending(ta => ta.CompletedAt)
                .FirstOrDefaultAsync();

            // 7. Recent Notifications
            var notifications = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(4)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Enrollments = enrollments;
            ViewBag.CompletedLessonIds = completedLessonIds;
            ViewBag.UpcomingClasses = upcomingClasses;
            ViewBag.PendingExercises = pendingExercises;
            ViewBag.RecentSubmissions = recentSubmissions;
            ViewBag.LatestTestAttempt = latestTestAttempt;
            ViewBag.Notifications = notifications;

            return View();
        }

        // GET: /Student/MyCourses
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Teacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c!.Chapters)
                        .ThenInclude(ch => ch.Lessons)
                .Where(e => e.StudentId == user.Id && e.PaymentStatus == "Completed")
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var progressList = await _context.LessonProgresses
                .Where(lp => lp.StudentId == user.Id)
                .ToListAsync();

            ViewBag.ProgressList = progressList;
            return View(enrollments);
        }

        // GET: /Student/Lesson/5
        [HttpGet]
        public async Task<IActionResult> Lesson(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var lesson = await _context.Lessons
                .Include(l => l.Chapter)
                    .ThenInclude(ch => ch!.Course)
                        .ThenInclude(c => c!.Chapters)
                            .ThenInclude(ch2 => ch2.Lessons)
                                .ThenInclude(l2 => l2.Exercises)
                .Include(l => l.Exercises)
                    .ThenInclude(ex => ex.Questions)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null) return NotFound();

            var courseId = lesson.Chapter!.CourseId;

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";
            bool isCourseTeacher = lesson.Chapter.Course?.TeacherId == user.Id;

            // Check enrollment or free preview or teacher privilege
            bool isEnrolled = isTeacher || isAdmin || isCourseTeacher || 
                await _context.Enrollments.AnyAsync(e => e.StudentId == user.Id && e.CourseId == courseId && e.PaymentStatus == "Completed");

            if (!isEnrolled && !lesson.IsFreePreview)
            {
                TempData["ErrorMessage"] = "Bạn cần mua khóa học này để truy cập bài học.";
                return RedirectToAction("Detail", "Course", new { id = courseId });
            }

            // Track / Update progress
            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == user.Id && lp.LessonId == id);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = user.Id,
                    LessonId = id,
                    IsCompleted = false,
                    LastAccessedAt = DateTime.Now
                };
                _context.LessonProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }
            else
            {
                progress.LastAccessedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            // All lessons in course in order
            var allLessons = lesson.Chapter.Course!.Chapters
                .OrderBy(c => c.OrderIndex)
                .SelectMany(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .ToList();

            var currentIndex = allLessons.FindIndex(l => l.LessonId == id);
            var prevLesson = currentIndex > 0 ? allLessons[currentIndex - 1] : null;
            var nextLesson = currentIndex >= 0 && currentIndex < allLessons.Count - 1 ? allLessons[currentIndex + 1] : null;

            var completedLessonIds = await _context.LessonProgresses
                .Where(lp => lp.StudentId == user.Id && lp.IsCompleted)
                .Select(lp => lp.LessonId)
                .ToListAsync();

            ViewBag.IsCompleted = progress.IsCompleted;
            ViewBag.PrevLesson = prevLesson;
            ViewBag.NextLesson = nextLesson;
            ViewBag.CompletedLessonIds = completedLessonIds;

            return View(lesson);
        }

        // POST: /Student/ToggleCompleteLesson
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleCompleteLesson(int lessonId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var progress = await _context.LessonProgresses
                .FirstOrDefaultAsync(lp => lp.StudentId == user.Id && lp.LessonId == lessonId);

            if (progress == null)
            {
                progress = new LessonProgress
                {
                    StudentId = user.Id,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedAt = DateTime.Now,
                    LastAccessedAt = DateTime.Now
                };
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = !progress.IsCompleted;
                progress.CompletedAt = progress.IsCompleted ? DateTime.Now : null;
                progress.LastAccessedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = progress.IsCompleted 
                ? "Tuyệt vời! Bạn đã hoàn thành bài học này." 
                : "Đã hủy đánh dấu hoàn thành bài học.";

            return RedirectToAction(nameof(Lesson), new { id = lessonId });
        }

        // GET: /Student/Exercise/5
        [HttpGet]
        public async Task<IActionResult> Exercise(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var exercise = await _context.Exercises
                .Include(ex => ex.Lesson)
                    .ThenInclude(l => l!.Chapter)
                        .ThenInclude(ch => ch!.Course)
                .Include(ex => ex.Questions)
                .FirstOrDefaultAsync(ex => ex.ExerciseId == id);

            if (exercise == null) return NotFound();

            var submission = await _context.ExerciseSubmissions
                .FirstOrDefaultAsync(s => s.ExerciseId == id && s.StudentId == user.Id);

            ViewBag.Submission = submission;
            return View(exercise);
        }

        // POST: /Student/SubmitExercise
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitExercise(int exerciseId, IFormCollection form)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var exercise = await _context.Exercises
                .Include(ex => ex.Questions)
                .FirstOrDefaultAsync(ex => ex.ExerciseId == exerciseId);

            if (exercise == null) return NotFound();

            int totalQ = exercise.Questions.Count;
            decimal totalMcPoints = 0m;
            decimal earnedMcPoints = 0m;
            int mcCount = 0;
            int mcCorrect = 0;
            bool hasWriting = false;
            var answersLog = new Dictionary<string, string>();

            foreach (var q in exercise.Questions)
            {
                string key = $"q_{q.QuestionId}";
                string ans = form[key].ToString().Trim();
                answersLog[key] = ans;

                decimal qPoints = q.Points > 0 ? q.Points : 10m;
                bool isEssay = q.Format == QuestionFormat.Writing || q.Format == QuestionFormat.Speaking;

                if (!isEssay)
                {
                    mcCount++;
                    totalMcPoints += qPoints;

                    if (!string.IsNullOrEmpty(ans) && !string.IsNullOrEmpty(q.CorrectAnswer))
                    {
                        bool isCorrect = false;
                        if (q.Format == QuestionFormat.MultipleChoice || q.Format == QuestionFormat.TrueFalse)
                        {
                            isCorrect = q.CorrectAnswer.Trim().StartsWith(ans.Substring(0, Math.Min(2, ans.Length)), StringComparison.OrdinalIgnoreCase)
                                     || string.Equals(q.CorrectAnswer.Trim(), ans, StringComparison.OrdinalIgnoreCase);
                        }
                        else
                        {
                            isCorrect = string.Equals(q.CorrectAnswer.Trim(), ans, StringComparison.OrdinalIgnoreCase);
                        }

                        if (isCorrect)
                        {
                            mcCorrect++;
                            earnedMcPoints += qPoints;
                        }
                    }
                }
                else
                {
                    hasWriting = true;
                }
            }

            decimal autoScore = earnedMcPoints;

            var existingSubmission = await _context.ExerciseSubmissions
                .FirstOrDefaultAsync(s => s.ExerciseId == exerciseId && s.StudentId == user.Id);

            string earnedStr = (autoScore % 1 == 0) ? ((int)autoScore).ToString() : autoScore.ToString("F1");
            string totalStr = (totalMcPoints % 1 == 0) ? ((int)totalMcPoints).ToString() : totalMcPoints.ToString("F1");
            string mcScoreDisplay = totalMcPoints > 0 ? $"{earnedStr}/{totalStr}" : earnedStr;

            string feedback = hasWriting 
                ? "Đã nộp bài thành công! Phần câu hỏi tự luận đang chờ giảng viên chấm điểm chi tiết." 
                : (totalMcPoints > 0 && autoScore >= (totalMcPoints * 0.8m) ? "Hoàn thành xuất sắc bài tập!" : "Đạt yêu cầu. Hãy xem lại các câu trả lời chưa đúng.");

            if (existingSubmission == null)
            {
                var submission = new ExerciseSubmission
                {
                    ExerciseId = exerciseId,
                    StudentId = user.Id,
                    SubmittedAt = DateTime.Now,
                    AutoScore = autoScore,
                    TotalScore = autoScore,
                    TeacherScore = null,
                    AnswersJson = JsonSerializer.Serialize(answersLog),
                    TeacherComment = feedback
                };
                _context.ExerciseSubmissions.Add(submission);
            }
            else
            {
                existingSubmission.SubmittedAt = DateTime.Now;
                existingSubmission.AutoScore = autoScore;
                existingSubmission.TotalScore = autoScore;
                existingSubmission.TeacherScore = null;
                existingSubmission.AnswersJson = JsonSerializer.Serialize(answersLog);
                existingSubmission.TeacherComment = feedback;
            }

            // Notification
            var notif = new Notification
            {
                UserId = user.Id,
                Title = "Bài tập đã được ghi nhận nộp bài",
                Content = hasWriting 
                    ? $"Bài tập '{exercise.Title}' đã nộp thành công (Điểm trắc nghiệm: {mcScoreDisplay}, đang chờ giảng viên chấm phần tự luận)." 
                    : $"Bạn đạt {mcScoreDisplay} trong bài tập '{exercise.Title}'.",
                Type = "ExerciseDue",
                LinkUrl = "/Student/Exercise/" + exerciseId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notif);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = hasWriting
                ? $"Nộp bài thành công! Điểm trắc nghiệm: {mcScoreDisplay}. Phần tự luận đang chờ GV chấm."
                : $"Nộp bài thành công! Điểm trắc nghiệm: {mcScoreDisplay}.";
            return RedirectToAction(nameof(Exercise), new { id = exerciseId });
        }

        // GET: /Student/Schedule
        [HttpGet]
        public async Task<IActionResult> Schedule()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            bool isTeacher = await _userManager.IsInRoleAsync(user, "Teacher") || user.RoleName == "Teacher";
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin") || user.RoleName == "Admin";

            if (isTeacher || isAdmin)
            {
                var myTaughtClasses = await _context.LiveClasses
                    .Include(lc => lc.Teacher)
                    .Include(lc => lc.Course)
                    .Where(lc => isAdmin || lc.TeacherId == user.Id)
                    .OrderBy(lc => lc.ScheduledDate)
                    .ThenBy(lc => lc.StartTime)
                    .ToListAsync();

                return View(myTaughtClasses);
            }

            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == user.Id && e.PaymentStatus == "Completed")
                .Select(e => e.CourseId)
                .ToListAsync();

            var myClasses = await _context.LiveClasses
                .Include(lc => lc.Teacher)
                .Include(lc => lc.Course)
                .Where(lc => lc.IsPublic || (lc.CourseId != null && enrolledCourseIds.Contains(lc.CourseId.Value)))
                .OrderBy(lc => lc.ScheduledDate)
                .ThenBy(lc => lc.StartTime)
                .ToListAsync();

            return View(myClasses);
        }
    }
}

