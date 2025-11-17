using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Marketplace.Controllers
{
  
    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Marketplace.Services.IEmailSender? _emailSender;

        public AdministradorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, Marketplace.Services.IEmailSender? emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        // Ação para a página principal do painel de administração, que agora contém todas as secções.
        public IActionResult Index()
        {
            return View();
        }

        // GET: Administrador/ValidarVendedores
        [HttpGet]
        public async Task<IActionResult> ValidarVendedores()
        {
            var pendentes = await _db.Vendedores
                .Where(v => v.Estado == null || v.Estado == "Pendente")
                .OrderBy(v => v.Nome)
                .ToListAsync();
            return View(pendentes);
        }

        // POST: Administrador/AprovarVendedor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarVendedor(int id)
        {
            var vendedor = await _db.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();
            vendedor.Estado = "Ativo";
            await _db.SaveChangesAsync();
            TempData["ValidarVendMsg"] = $"Vendedor '{vendedor.Nome}' aprovado.";
            // Notificar por email
            try
            {
                await (_emailSender?.SendAsync(vendedor.Email, "Aprovação de Vendedor - DriveDeal",
                    $"<p>Olá {vendedor.Nome},</p><p>O seu pedido para vendedor foi <strong>aprovado</strong>.</p><p>Já pode criar e gerir anúncios no DriveDeal.</p>" ) ?? Task.CompletedTask);
            }
            catch { }
            return RedirectToAction(nameof(ValidarVendedores));
        }

        // POST: Administrador/RejeitarVendedor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarVendedor(int id)
        {
            var vendedor = await _db.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();
            vendedor.Estado = "Rejeitado";
            await _db.SaveChangesAsync();
            TempData["ValidarVendMsg"] = $"Vendedor '{vendedor.Nome}' rejeitado.";
            // Notificar por email
            try
            {
                await (_emailSender?.SendAsync(vendedor.Email, "Rejeição de Vendedor - DriveDeal",
                    $"<p>Olá {vendedor.Nome},</p><p>O seu pedido para vendedor foi <strong>rejeitado</strong>.</p><p>Contacte o suporte caso necessite de esclarecimentos.</p>") ?? Task.CompletedTask);
            }
            catch { }
            return RedirectToAction(nameof(ValidarVendedores));
        }

        // Pedidos de Vendedor baseados em Claims
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarPedidoVendedor(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var c in claims.Where(c => c.Type == "SellerRequest"))
                await _userManager.RemoveClaimAsync(user, c);

            // Garantir role Vendedor
            if (!await _userManager.IsInRoleAsync(user, "Vendedor"))
                await _userManager.AddToRoleAsync(user, "Vendedor");

            TempData["ValidarVendMsg"] = $"Pedido de vendedor aprovado para {user.Email}.";
            // Notificar por email
            try
            {
                await (_emailSender?.SendAsync(user.Email!, "Aprovação de Pedido - DriveDeal",
                    $"<p>Olá {user.FullName ?? user.UserName},</p><p>O seu pedido para se tornar vendedor foi <strong>aprovado</strong>.</p><p>Já pode criar e gerir anúncios no DriveDeal.</p>") ?? Task.CompletedTask);
            }
            catch { }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarPedidoVendedor(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return NotFound();
            var claims = await _userManager.GetClaimsAsync(user);
            foreach (var c in claims.Where(c => c.Type == "SellerRequest"))
                await _userManager.RemoveClaimAsync(user, c);
            TempData["ValidarVendMsg"] = $"Pedido de vendedor rejeitado para {user.Email}.";
            // Notificar por email
            try
            {
                await (_emailSender?.SendAsync(user.Email!, "Rejeição de Pedido - DriveDeal",
                    $"<p>Olá {user.FullName ?? user.UserName},</p><p>O seu pedido para se tornar vendedor foi <strong>rejeitado</strong>.</p><p>Contacte o suporte caso necessite de esclarecimentos.</p>") ?? Task.CompletedTask);
            }
            catch { }
            return RedirectToAction(nameof(Index));
        }
    }
}

