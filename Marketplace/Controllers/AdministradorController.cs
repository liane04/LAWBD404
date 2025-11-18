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
        public async Task<IActionResult> Index()
        {
            // Carregar vendedores pendentes para exibir no painel
            var vendedoresPendentes = await _db.Vendedores
                .Where(v => v.Estado == null || v.Estado == "Pendente")
                .OrderBy(v => v.Nome)
                .ToListAsync();

            ViewBag.VendedoresPendentes = vendedoresPendentes;
            ViewBag.TotalPendentes = vendedoresPendentes.Count;

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

            TempData["Success"] = $"Vendedor '{vendedor.Nome}' aprovado com sucesso!";

            // Notificar vendedor por email
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(vendedor.Email))
                {
                    await _emailSender.SendAsync(
                        vendedor.Email,
                        "Aprovação de Vendedor - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #0d6efd;'>Parabéns! Seu pedido foi aprovado</h2>
                            <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                            <p>O seu pedido para se tornar vendedor na plataforma <strong>404 Ride</strong> foi <span style='color: #28a745;'>aprovado</span>.</p>
                            <p>Já pode começar a criar e gerir os seus anúncios de veículos!</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the approval
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
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

            TempData["Warning"] = $"Vendedor '{vendedor.Nome}' foi rejeitado.";

            // Notificar vendedor por email
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(vendedor.Email))
                {
                    await _emailSender.SendAsync(
                        vendedor.Email,
                        "Pedido de Vendedor - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #dc3545;'>Pedido não aprovado</h2>
                            <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                            <p>Lamentamos informar que o seu pedido para se tornar vendedor na plataforma <strong>404 Ride</strong> não foi aprovado neste momento.</p>
                            <p>Se necessitar de esclarecimentos adicionais, por favor contacte o nosso suporte.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the rejection
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

