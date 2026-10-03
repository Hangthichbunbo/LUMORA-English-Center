using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
