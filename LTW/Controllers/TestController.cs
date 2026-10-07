using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
