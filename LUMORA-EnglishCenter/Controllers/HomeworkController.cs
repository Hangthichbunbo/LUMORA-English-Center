using Microsoft.AspNetCore.Mvc;

namespace LUMORA_EnglishCenter.Controllers
{
    public class HomeworkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
