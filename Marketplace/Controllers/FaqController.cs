using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    public class FaqController : Controller
    {
        // GET: /Faq
        public IActionResult Index()
        {
            ViewData["Title"] = "FAQ";
            return View();
        }
    }
}

