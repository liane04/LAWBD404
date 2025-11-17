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
            var vendedores = await _db.Vendedores
                .AsNoTracking()
                .OrderBy(v => v.Nome)
                .ToListAsync();

            var pendentes = vendedores.Where(v => string.IsNullOrWhiteSpace(v.Estado) || v.Estado == "Pendente").ToList();
            var aprovados = vendedores.Where(v => v.Estado == "Ativo").Count();
            var rejeitados = vendedores.Where(v => v.Estado == "Rejeitado").Count();

            // Pedidos de vendedor via Claims (SellerRequest=Pendente)
            var claimPendentes = await _db.Set<IdentityUserClaim<int>>()
                .Where(c => c.ClaimType == "SellerRequest" && c.ClaimValue == "Pendente")
                .ToListAsync();
            var userIds = claimPendentes.Select(c => c.UserId).Distinct().ToList();
            var users = await _db.Set<ApplicationUser>()
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var pedidos = users.Select(u => new PedidoVendedorVM
            {
                UserId = u.Id,
                Nome = u.FullName ?? u.UserName ?? ("User " + u.Id),
                Email = u.Email ?? "",
                Username = u.UserName ?? "",
            }).ToList();

            var model = new ValidarVendedoresSectionVM
            {
                Pendentes = pendentes,
                Todos = vendedores,
                TotalPendentes = pendentes.Count + pedidos.Count,
                TotalAprovados = aprovados,
                TotalRejeitados = rejeitados,
                Pedidos = pedidos
            };

            return View(model);
        }
    }

    public class ValidarVendedoresSectionVM
    {
        public System.Collections.Generic.List<Marketplace.Models.Vendedor> Pendentes { get; set; } = new();
        public System.Collections.Generic.List<Marketplace.Models.Vendedor> Todos { get; set; } = new();
        public int TotalPendentes { get; set; }
        public int TotalAprovados { get; set; }
        public int TotalRejeitados { get; set; }
        public System.Collections.Generic.List<PedidoVendedorVM> Pedidos { get; set; } = new();
    }

    public class PedidoVendedorVM
    {
        public int UserId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
