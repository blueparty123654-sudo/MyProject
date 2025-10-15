using Microsoft.AspNetCore.Mvc;

namespace MyProject.Controllers
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
