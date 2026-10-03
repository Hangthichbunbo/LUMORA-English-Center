using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
