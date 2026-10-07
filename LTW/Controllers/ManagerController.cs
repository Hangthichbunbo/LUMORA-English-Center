using Microsoft.AspNetCore.Mvc;

namespace LTW.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
