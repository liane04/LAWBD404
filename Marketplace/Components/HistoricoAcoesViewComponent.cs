using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Components
{
    public class HistoricoAcoesViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public HistoricoAcoesViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var historico = await _db.Set<HistoricoAcao>()
                .Include(h => h.Administrador)
                .Include(h => (h as AcaoUser).Utilizador)
                .Include(h => (h as AcaoAnuncio).Anuncio)
                .OrderByDescending(h => h.Data)
                .Take(50) // Limit to last 50 actions for performance
                .ToListAsync();

            return View(historico);
        }
    }
}
