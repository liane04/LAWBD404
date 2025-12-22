using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Marketplace.Components
{
    public class EstatisticasViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public EstatisticasViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var hoje = DateTime.Today;
            var seisMesesAtras = new DateTime(hoje.Year, hoje.Month, 1).AddMonths(-5);

            // 1. Sales Data (Last 6 Months)
            var rawSalesData = await _db.Compras
                .Include(c => c.Anuncio)
                .Where(c => c.Data >= seisMesesAtras)
                .Select(c => new { c.Data, c.Anuncio.Preco })
                .ToListAsync();

            var salesGrouped = rawSalesData
                .GroupBy(c => new { c.Data.Year, c.Data.Month })
                .Select(g => new
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalValue = g.Sum(c => c.Preco),
                    TotalCount = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Prepare labels for the last 6 months (even if no sales)
            var labels = new List<string>();
            var values = new List<decimal>();
            var counts = new List<int>();

            for (int i = 0; i < 6; i++)
            {
                var d = seisMesesAtras.AddMonths(i);
                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(d.Month);
                labels.Add($"{monthName} {d.Year}");

                var record = salesGrouped.FirstOrDefault(r => r.Date.Year == d.Year && r.Date.Month == d.Month);
                values.Add(record?.TotalValue ?? 0);
                counts.Add(record?.TotalCount ?? 0);
            }

            // 2. Category Distribution (Active Ads)
            var categoryData = await _db.Anuncios
                .Include(a => a.Categoria)
                .GroupBy(a => a.Categoria.Nome)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(5)
                .ToListAsync();

            var model = new EstatisticasVM
            {
                Labels = labels,
                SalesValueData = values,
                SalesCountData = counts,
                CategoryLabels = categoryData.Select(x => x.Name).ToList(),
                CategoryData = categoryData.Select(x => x.Count).ToList()
            };

            return View(model);
        }
    }

    public class EstatisticasVM
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> SalesValueData { get; set; } = new();
        public List<int> SalesCountData { get; set; } = new();
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryData { get; set; } = new();
    }
}
