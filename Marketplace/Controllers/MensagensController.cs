using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{

    public class MensagensController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}

