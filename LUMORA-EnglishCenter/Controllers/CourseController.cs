using Microsoft.AspNetCore.Mvc;

namespace LUMORA_EnglishCenter.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
