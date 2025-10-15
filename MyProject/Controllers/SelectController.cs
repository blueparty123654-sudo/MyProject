using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    public class SelectController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
