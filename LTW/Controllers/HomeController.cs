// ============================================================================
// Main Author: hoangthuhang
// Project: LTW Project - IELTS Online Tests Landing Page System
// Architecture: ASP.NET Core MVC (Model - View - Controller)
// File: Controllers/HomeController.cs
// Description: Controller chính xử lý điều hướng, tiếp nhận thông tin người dùng
//              và cung cấp dữ liệu cho toàn bộ giao diện 
// ============================================================================

using System.Diagnostics;
using LTW.Models;
using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
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
            // Trường Message là tùy chọn (không bắt buộc), loại bỏ lỗi xác thực nếu người dùng để trống
            ModelState.Remove("ConsultationForm.Message");
            ModelState.Remove($"{nameof(incomingModel.ConsultationForm)}.{nameof(incomingModel.ConsultationForm.Message)}");

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                         Request.Headers.Accept.ToString().Contains("application/json");

            if (ModelState.IsValid)
            {
                var message = $"Cảm ơn {incomingModel.ConsultationForm.FullName}! Chuyên viên IELTS của chúng tôi sẽ liên hệ tư vấn lộ trình Band {incomingModel.ConsultationForm.TargetBand} trong vòng 24 giờ.";

                if (isAjax)
                {
                    return Json(new { success = true, message = message });
                }

                TempData["ConsultationSuccess"] = message;
                return RedirectToAction(nameof(Index), "Home", null, "contact-section");
            }

            var errorMessage = "Vui lòng kiểm tra lại các trường thông tin bắt buộc.";

            if (isAjax)
            {
                return Json(new { success = false, message = errorMessage });
            }

            // Nếu gửi theo dạng form truyền thống không dùng JS
            var viewModel = new LandingPageViewModel
            {
                ConsultationForm = incomingModel.ConsultationForm
            };
            TempData["ConsultationError"] = errorMessage;
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

