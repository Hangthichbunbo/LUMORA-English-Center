using Microsoft.AspNetCore.Mvc;

namespace LUMORA_EnglishCenter.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
