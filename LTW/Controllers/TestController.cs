using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LTW.Data;
using LTW.Models;
using System.Text.Json;

namespace LTW.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Test/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var tests = await _context.Tests
                .Include(t => t.Questions)
                .Where(t => t.IsActive)
                .OrderBy(t => t.Type)
                .ToListAsync();

            return View(tests);
        }

        // GET: /Test/ExampleTest
        [HttpGet]
        public async Task<IActionResult> ExampleTest()
        {
            var test = await _context.Tests
                .Include(t => t.Questions.OrderBy(q => q.Skill).ThenBy(q => q.TestQuestionId))
                .FirstOrDefaultAsync(t => t.Type == ExamType.ExampleTest && t.IsActive);

            if (test == null)
            {
                test = await _context.Tests
                    .Include(t => t.Questions)
                    .FirstOrDefaultAsync();
            }

            if (test == null) return NotFound("Chưa có bài thi trong hệ thống.");

            return View("TakeTest", test);
        }

        // GET: /Test/PlacementTest
        [HttpGet]
        public async Task<IActionResult> PlacementTest()
        {
            var test = await _context.Tests
                .Include(t => t.Questions.OrderBy(q => q.Skill).ThenBy(q => q.TestQuestionId))
                .FirstOrDefaultAsync(t => t.Type == ExamType.PlacementTest && t.IsActive);

            if (test == null)
            {
                test = await _context.Tests
                    .Include(t => t.Questions)
                    .FirstOrDefaultAsync();
            }

            if (test == null) return NotFound("Chưa có bài kiểm tra xếp lớp.");

            return View("TakeTest", test);
        }

        // POST: /Test/SubmitTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int testId, string fullName, string email, IFormCollection form)
        {
            var test = await _context.Tests
                .Include(t => t.Questions)
                .FirstOrDefaultAsync(t => t.TestId == testId);

            if (test == null) return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            var currentUser = currentUserId != null ? await _userManager.FindByIdAsync(currentUserId) : null;

            if (string.IsNullOrEmpty(fullName) && currentUser != null)
            {
                fullName = currentUser.FullName;
            }
            if (string.IsNullOrEmpty(email) && currentUser != null)
            {
                email = currentUser.Email ?? "";
            }

            if (string.IsNullOrWhiteSpace(fullName)) fullName = "Candidate";
            if (string.IsNullOrWhiteSpace(email)) email = "student@example.com";

            int totalListening = 0, correctListening = 0;
            int totalReading = 0, correctReading = 0;
            var answersLog = new Dictionary<string, string>();

            foreach (var q in test.Questions)
            {
                string key = $"q_{q.TestQuestionId}";
                string userAnswer = form[key].ToString().Trim();
                answersLog[key] = userAnswer;

                if (q.Skill == SkillType.Listening)
                {
                    totalListening++;
                    if (!string.IsNullOrEmpty(userAnswer) && q.CorrectAnswer.Trim().StartsWith(userAnswer.Substring(0, Math.Min(2, userAnswer.Length))))
                    {
                        correctListening++;
                    }
                }
                else if (q.Skill == SkillType.Reading)
                {
                    totalReading++;
                    if (!string.IsNullOrEmpty(userAnswer) && q.CorrectAnswer.Trim().StartsWith(userAnswer.Substring(0, Math.Min(2, userAnswer.Length))))
                    {
                        correctReading++;
                    }
                }
            }

            // Scoring Algorithm (IELTS Band scale 0 - 9)
            decimal listeningBand = totalListening > 0 
                ? Math.Round((decimal)correctListening / totalListening * 4.0m + 5.0m, 1) 
                : 6.5m;
            decimal readingBand = totalReading > 0 
                ? Math.Round((decimal)correctReading / totalReading * 4.0m + 5.0m, 1) 
                : 6.5m;

            // Writing assessment based on text length & structure
            string writingAnswer = form["q_writing"].ToString();
            decimal writingBand = 6.0m;
            if (writingAnswer.Length > 200) writingBand = 7.0m;
            else if (writingAnswer.Length > 100) writingBand = 6.5m;

            // Speaking simulated assessment
            decimal speakingBand = 6.5m;

            // IELTS Standard Rounding: nearest 0.5
            decimal rawAvg = (listeningBand + readingBand + writingBand + speakingBand) / 4.0m;
            decimal overallBand = Math.Round(rawAvg * 2, MidpointRounding.AwayFromZero) / 2;
            if (overallBand > 9.0m) overallBand = 9.0m;

            // Estimated CEFR / IELTS Level & Feedback (English Only per Rule 9 & 15)
            string estimatedLevel;
            string feedback;
            if (overallBand >= 7.5m)
            {
                estimatedLevel = "C1 Advanced (IELTS 7.5+)";
                feedback = "Outstanding proficiency! You demonstrate strong comprehension and effective language control. To reach Band 8.0 - 8.5, focus on natural fluency in Speaking Part 3 and sophisticated cohesion in Writing Task 2.";
            }
            else if (overallBand >= 6.5m)
            {
                estimatedLevel = "B2 Upper-Intermediate (IELTS 6.5)";
                feedback = "Solid foundation! You handle standard Listening and Reading tasks effectively, though caution is advised with complex paraphrasing. For Writing and Speaking, expanding topic-specific collocations and complex sentence patterns will help elevate your score.";
            }
            else
            {
                estimatedLevel = "B1 Intermediate (IELTS 5.5)";
                feedback = "You demonstrate fundamental vocabulary and grammar control. Recommended focus areas include targeted academic grammar, dictation practice to improve listening accuracy, and skimming/scanning strategies for reading speed.";
            }

            // Recommended courses
            var recommendedCourses = await _context.Courses
                .Where(c => c.IsPublished)
                .OrderByDescending(c => c.IsFeatured)
                .Take(2)
                .Select(c => new { c.CourseId, c.Title, c.Thumbnail, c.Price, c.Level })
                .ToListAsync();

            var attempt = new TestAttempt
            {
                TestId = test.TestId,
                StudentId = currentUserId,
                FullName = fullName,
                Email = email,
                StartedAt = DateTime.Now.AddMinutes(-test.TimeLimitMinutes),
                CompletedAt = DateTime.Now,
                ListeningScore = listeningBand,
                ReadingScore = readingBand,
                WritingScore = writingBand,
                SpeakingScore = speakingBand,
                OverallScore = overallBand,
                EstimatedLevel = estimatedLevel,
                Feedback = feedback,
                AnswersJson = JsonSerializer.Serialize(answersLog),
                RecommendedCoursesJson = JsonSerializer.Serialize(recommendedCourses)
            };

            _context.TestAttempts.Add(attempt);

            if (currentUser != null)
            {
                var notif = new Notification
                {
                    UserId = currentUser.Id,
                    Title = "Kết quả bài kiểm tra năng lực tiếng Anh",
                    Content = $"Bạn đã hoàn thành bài thi '{test.Title}' với Band điểm tổng thể đạt {overallBand:F1}. Xem phân tích chi tiết ngay!",
                    Type = "TestResult",
                    LinkUrl = "/Test/Result/" + attempt.AttemptId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notif);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Result), new { id = attempt.AttemptId });
        }

        // GET: /Test/Result/5
        [HttpGet]
        public async Task<IActionResult> Result(int id)
        {
            var attempt = await _context.TestAttempts
                .Include(a => a.Test)
                .FirstOrDefaultAsync(a => a.AttemptId == id);

            if (attempt == null)
            {
                return NotFound();
            }

            return View(attempt);
        }
    }
}

