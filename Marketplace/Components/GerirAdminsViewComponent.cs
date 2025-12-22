using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Components
{
    public class GerirAdminsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public GerirAdminsViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var admins = await _db.Administradores
                .Include(a => a.IdentityUser)
                .Select(a => new AdminDetalhadoVM
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Username = a.Username,
                    Email = a.Email,
                    NivelAcesso = a.NivelAcesso ?? "Nivel 2",
                    ImagemPerfil = a.ImagemPerfil,
                    IdentityId = a.IdentityUserId
                })
                .ToListAsync();

            var model = new GerirAdminsVM
            {
                Admins = admins,
                TotalAdmins = admins.Count
            };

            return View(model);
        }
    }

    public class GerirAdminsVM
    {
        public List<AdminDetalhadoVM> Admins { get; set; } = new();
        public int TotalAdmins { get; set; }
    }

    public class AdminDetalhadoVM
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NivelAcesso { get; set; } = string.Empty;
        public string? ImagemPerfil { get; set; }
        public int IdentityId { get; set; }
    }
}
