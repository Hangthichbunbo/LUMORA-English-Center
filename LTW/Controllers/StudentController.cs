using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
