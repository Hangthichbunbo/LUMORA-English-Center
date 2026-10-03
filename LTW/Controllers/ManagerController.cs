using Microsoft.AspNetCore.Mvc;

namespace LTW_5.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
