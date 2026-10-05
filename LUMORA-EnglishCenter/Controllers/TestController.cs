using Microsoft.AspNetCore.Mvc;

namespace LUMORA_EnglishCenter.Controllers
{
    public class TestController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
