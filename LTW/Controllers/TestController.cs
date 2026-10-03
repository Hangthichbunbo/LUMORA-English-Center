using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
