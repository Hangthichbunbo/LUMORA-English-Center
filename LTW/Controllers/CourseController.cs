using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class CourseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
