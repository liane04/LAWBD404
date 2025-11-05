using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.Controllers
{
  
    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        // Ação para a página principal do painel de administração, que agora contém todas as secções.
        public IActionResult Index()
        {
            return View();
        }
    }
}

