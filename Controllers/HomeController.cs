using Microsoft.AspNetCore.Mvc;

namespace MyShop1.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }

}