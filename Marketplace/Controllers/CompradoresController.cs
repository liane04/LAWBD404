using Marketplace.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    public class CompradoresController : Controller
    {
        private readonly MarketplaceContext _context;

        public CompradoresController(MarketplaceContext context)
        {
            _context = context;
        }


        // GET: CompradoresController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CompradoresController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CompradoresController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CompradoresController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CompradoresController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CompradoresController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CompradoresController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CompradoresController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Encontrar o comprador que vai ser apagado.
            var comprador = await _context.Comprador.FindAsync(id);

            if (comprador != null)
            {
                // 2. A "CASCATA MANUAL": Encontrar todos os favoritos deste comprador.
                var favoritosParaApagar = await _context.AnuncioFav
                    .Where(af => af.CompradorId == id)
                    .ToListAsync();

                // 3. Apagar todos os favoritos encontrados.
                if (favoritosParaApagar.Any())
                {
                    _context.AnuncioFav.RemoveRange(favoritosParaApagar);
                }

                // 4. Agora que os favoritos já não existem, apagar o comprador.
                _context.Comprador.Remove(comprador);

                // 5. Salvar todas as alterações (apagar favoritos E o comprador) de uma só vez.
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home"); // Redirecionar para a página inicial
        }

    }
}
