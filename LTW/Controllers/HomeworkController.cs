using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class HomeworkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
