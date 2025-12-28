using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Marketplace.Models;

namespace Marketplace.Components
{
    public class ValidarVendedoresViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ValidarVendedoresViewComponent(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var pedidos = await _db.PedidosVendedor
                .Include(p => p.Comprador)
                .AsNoTracking()
                .OrderByDescending(p => p.DataPedido)
                .ToListAsync();

            var model = new ValidarVendedoresSectionVM
            {
                Pedidos = pedidos,
                TotalPendentes = pedidos.Count(p => p.Estado == "Pendente"),
                TotalAprovados = pedidos.Count(p => p.Estado == "Aprovado"),
                TotalRejeitados = pedidos.Count(p => p.Estado == "Rejeitado")
            };

            return View(model);
        }
    }

    public class ValidarVendedoresSectionVM
    {
        public System.Collections.Generic.List<Marketplace.Models.PedidoVendedor> Pedidos { get; set; } = new();
        public int TotalPendentes { get; set; }
        public int TotalAprovados { get; set; }
        public int TotalRejeitados { get; set; }
    }
}
