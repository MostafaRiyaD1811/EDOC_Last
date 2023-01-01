using Microsoft.AspNetCore.Mvc;

namespace Document.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult NoFound()
        {
            return View();
        }
    }
}
