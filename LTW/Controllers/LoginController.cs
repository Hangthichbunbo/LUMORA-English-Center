using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
