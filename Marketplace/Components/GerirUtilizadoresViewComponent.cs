using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Marketplace.Models;

namespace Marketplace.Components
{
    public class GerirUtilizadoresViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public GerirUtilizadoresViewComponent(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Obter todos os utilizadores do Identity
            var allUsers = await _userManager.Users.ToListAsync();

            // Criar lista de utilizadores com informações detalhadas
            var utilizadoresDetalhados = new System.Collections.Generic.List<UtilizadorDetalhadoVM>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var isComprador = roles.Contains("Comprador");
                var isVendedor = roles.Contains("Vendedor");
                var isAdmin = roles.Contains("Administrador");

                string? imagemPerfil = null;
                string? nif = null;
                string? nivelAcesso = null;

                // Buscar dados adicionais de Vendedor ou Comprador ou Admin
                if (isAdmin)
                {
                    var admin = await _db.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);
                    if (admin != null)
                    {
                        nivelAcesso = admin.NivelAcesso;
                    }
                }
                
                if (isVendedor)
                {
                    var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
                    if (vendedor != null)
                    {
                        imagemPerfil = vendedor.ImagemPerfil;
                        nif = vendedor.Nif;
                    }
                }
                else if (isComprador)
                {
                    var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
                    if (comprador != null)
                    {
                        imagemPerfil = comprador.ImagemPerfil;
                    }
                }

                utilizadoresDetalhados.Add(new UtilizadorDetalhadoVM
                {
                    Id = user.Id,
                    Nome = user.FullName ?? user.UserName ?? "N/A",
                    Email = user.Email ?? "N/A",
                    UserName = user.UserName ?? "N/A",
                    IsComprador = isComprador,
                    IsVendedor = isVendedor,
                    IsAdministrador = isAdmin,
                    ImagemPerfil = imagemPerfil,
                    Nif = nif,
                    NivelAcesso = nivelAcesso,
                    EmailConfirmed = user.EmailConfirmed,
                    LockoutEnabled = user.LockoutEnabled,
                    LockoutEnd = user.LockoutEnd
                });
            }

            var model = new GerirUtilizadoresSectionVM
            {
                Utilizadores = utilizadoresDetalhados,
                TotalCompradores = utilizadoresDetalhados.Count(u => u.IsComprador),
                TotalVendedores = utilizadoresDetalhados.Count(u => u.IsVendedor),
                TotalAdministradores = utilizadoresDetalhados.Count(u => u.IsAdministrador),
                TotalBloqueados = utilizadoresDetalhados.Count(u => u.LockoutEnd.HasValue && u.LockoutEnd > System.DateTimeOffset.UtcNow)
            };

            ViewBag.NivelAcesso = ViewBag.NivelAcesso;
            return View(model);
        }
    }

    public class GerirUtilizadoresSectionVM
    {
        public System.Collections.Generic.List<UtilizadorDetalhadoVM> Utilizadores { get; set; } = new();
        public int TotalCompradores { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalAdministradores { get; set; }
        public int TotalBloqueados { get; set; }
    }

    public class UtilizadorDetalhadoVM
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public bool IsComprador { get; set; }
        public bool IsVendedor { get; set; }
        public bool IsAdministrador { get; set; }
        public string? ImagemPerfil { get; set; }
        public string? Nif { get; set; }
        public string? NivelAcesso { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }
        public System.DateTimeOffset? LockoutEnd { get; set; }

        public bool EstaBloqueado => LockoutEnd.HasValue && LockoutEnd > System.DateTimeOffset.UtcNow;
    }
}
