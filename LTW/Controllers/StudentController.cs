using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
