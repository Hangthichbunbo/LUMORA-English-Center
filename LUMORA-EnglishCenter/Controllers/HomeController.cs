// ============================================================================
// Main Author: hoangthuhang
// Project: LTW Project - IELTS Online Tests Landing Page System
// Architecture: ASP.NET Core MVC (Model - View - Controller)
// File: Controllers/HomeController.cs
// Description: Controller chính xử lý điều hướng, tiếp nhận thông tin người dùng
//              và cung cấp dữ liệu cho toàn bộ giao diện 
// ============================================================================

using System.Diagnostics;
using LUMORA_EnglishCenter.Models;
using Microsoft.AspNetCore.Mvc;

namespace LUMORA_EnglishCenter.Controllers
{
    /// <summary>
    /// HomeController đảm nhiệm tiếp nhận yêu cầu từ client, kết nối với dữ liệu ViewModel
    /// và hiển thị Landing Page hoàn chỉnh.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Constructor tiêm phụ thuộc ILogger theo chuẩn ASP.NET Core
        /// </summary>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Action Index: Trả về trang chủ Landing Page 
        /// </summary>
        [HttpGet]
        public IActionResult Index()
        {
            // Khởi tạo đối tượng ViewModel toàn diện
            var model = new LandingPageViewModel();
            return View(model);
        }

        /// <summary>
        /// Action xử lý gửi Form đăng ký tư vấn trực tuyến (Get In Touch Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SubmitConsultation(LandingPageViewModel incomingModel)
        {
            if (ModelState.IsValid)
            {
                // Giả lập lưu trữ thông tin đăng ký thành công vào cơ sở dữ liệu
                TempData["SuccessMessage"] = $"Cảm ơn {incomingModel.ConsultationForm.FullName}! Chuyên viên IELTS của chúng tôi sẽ liên hệ tư vấn lộ trình Band {incomingModel.ConsultationForm.TargetBand} trong vòng 24 giờ.";
                return RedirectToAction(nameof(Index), "Home", "contact-section");
            }

            // Nếu dữ liệu không hợp lệ, khởi tạo lại model và báo lỗi
            var viewModel = new LandingPageViewModel
            {
                ConsultationForm = incomingModel.ConsultationForm
            };
            TempData["ErrorMessage"] = "Vui lòng kiểm tra lại các trường thông tin bắt buộc.";
            return View("Index", viewModel);
        }

        /// <summary>
        /// Action ExamLibrary: Thư viện đề thi IELTS
        /// </summary>
        [HttpGet]
        public IActionResult ExamLibrary()
        {
            return RedirectToAction(nameof(Index), new { section = "materials" });
        }

        /// <summary>
        /// Action IeltsTips: Mẹo làm bài thi
        /// </summary>
        [HttpGet]
        public IActionResult IeltsTips()
        {
            return RedirectToAction(nameof(Index), new { section = "steps" });
        }

        /// <summary>
        /// Action IeltsPrep: Khóa học chuẩn bị
        /// </summary>
        [HttpGet]
        public IActionResult IeltsPrep()
        {
            return RedirectToAction(nameof(Index), new { section = "courses" });
        }

        /// <summary>
        /// Action LiveLessons: Lớp học trực tuyến
        /// </summary>
        [HttpGet]
        public IActionResult LiveLessons()
        {
            return RedirectToAction(nameof(Index), new { section = "live-lessons" });
        }

        /// <summary>
        /// Action IeltsCourses: Khóa học chuyên sâu
        /// </summary>
        [HttpGet]
        public IActionResult IeltsCourses()
        {
            return RedirectToAction(nameof(Index), new { section = "courses" });
        }

        /// <summary>
        /// Action Contact: Liên hệ
        /// </summary>
        [HttpGet]
        public IActionResult Contact()
        {
            return RedirectToAction(nameof(Index), new { section = "contact-section" });
        }

        /// <summary>
        /// Action Privacy: Chính sách bảo mật & Điều khoản sử dụng
        /// </summary>
        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Action Error: Bắt ngoại lệ và hiển thị giao diện báo lỗi
        /// </summary>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

