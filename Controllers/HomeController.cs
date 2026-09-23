using Microsoft.AspNetCore.Mvc;

namespace WholesaleHub.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
