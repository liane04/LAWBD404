using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
    // Com a nova abordagem de página única, este controller fica muito mais simples.
    // A sua única responsabilidade é carregar o painel de administração principal.
    public class AdministradorController : Controller
    {
        // Ação para a página principal do painel de administração, que agora contém todas as secções.
        public IActionResult Index()
        {
            return View();
        }
    }
}
