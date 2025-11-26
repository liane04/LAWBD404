using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Marketplace.Controllers
{
    [Authorize]
    public class MensagensController : Controller
    {

        public IActionResult Index()
        {
            return View();
        }
    }
}

