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

        public async Task<IViewComponentResult> InvokeAsync(int count = 50, bool isDashboard = false, string? filter = null)
        {
            var query = _db.Set<HistoricoAcao>()
                .Include(h => h.Administrador)
                .Include(h => (h as AcaoUser).Utilizador)
                .Include(h => (h as AcaoAnuncio).Anuncio)
                .AsQueryable();

            if (!string.IsNullOrEmpty(filter))
            {
                if (filter == "users")
                {
                    query = query.Where(h => h.TipoAcao == "AcaoUser");
                }
                else if (filter == "ads")
                {
                    query = query.Where(h => h.TipoAcao == "AcaoAnuncio");
                }
            }

            var historico = await query
                .OrderByDescending(h => h.Data)
                .Take(count)
                .ToListAsync();

            if (isDashboard)
            {
                return View("DashboardWidget", historico);
            }

            ViewBag.CurrentFilter = filter;
            return View(historico);
        }
    }
}
