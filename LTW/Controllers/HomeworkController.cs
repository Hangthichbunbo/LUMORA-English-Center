using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class HomeworkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
