using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Components
{
    public class CriarUtilizadorViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            ViewBag.NivelAcesso = ViewBag.NivelAcesso; // Ensure it's passed down
            return View();
        }
    }
}
