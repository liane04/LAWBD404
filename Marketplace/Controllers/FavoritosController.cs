using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Controllers
{
    [Authorize(Roles = "Comprador")]
    public class FavoritosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Favoritos - Lista todos os anúncios favoritos do comprador
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Utilizadores");
            }

            var comprador = await _context.Compradores
                .Include(c => c.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Marca)
                .Include(c => c.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Modelo)
                .Include(c => c.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Imagens)
                .Include(c => c.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Combustivel)
                .Include(c => c.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Vendedor)
                .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

            if (comprador == null)
            {
                return NotFound("Comprador não encontrado");
            }

            return View(comprador.AnunciosFavoritos.OrderByDescending(af => af.Id).ToList());
        }

        // POST: Favoritos/Toggle - Adiciona ou remove favorito (AJAX)
        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] int anuncioId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, message = "Utilizador não autenticado" });
                }

                var comprador = await _context.Compradores
                    .Include(c => c.AnunciosFavoritos)
                    .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

                if (comprador == null)
                {
                    return Json(new { success = false, message = "Comprador não encontrado" });
                }

                // Verifica se o anúncio existe
                var anuncio = await _context.Anuncios.FindAsync(anuncioId);
                if (anuncio == null)
                {
                    return Json(new { success = false, message = "Anúncio não encontrado" });
                }

                // Verifica se já é favorito
                var favoritoExistente = comprador.AnunciosFavoritos
                    .FirstOrDefault(af => af.AnuncioId == anuncioId);

                bool adicionado;

                if (favoritoExistente != null)
                {
                    // Remove dos favoritos
                    _context.AnunciosFavoritos.Remove(favoritoExistente);
                    adicionado = false;
                }
                else
                {
                    // Adiciona aos favoritos
                    var novoFavorito = new AnuncioFav
                    {
                        CompradorId = comprador.Id,
                        AnuncioId = anuncioId
                    };
                    _context.AnunciosFavoritos.Add(novoFavorito);
                    adicionado = true;
                }

                await _context.SaveChangesAsync();

                // Conta total de favoritos
                var totalFavoritos = await _context.AnunciosFavoritos
                    .CountAsync(af => af.CompradorId == comprador.Id);

                return Json(new
                {
                    success = true,
                    adicionado = adicionado,
                    totalFavoritos = totalFavoritos,
                    message = adicionado ? "Anúncio adicionado aos favoritos" : "Anúncio removido dos favoritos"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Erro: {ex.Message}" });
            }
        }

        // GET: Favoritos/Check - Verifica se anúncio é favorito (AJAX)
        [HttpGet]
        public async Task<IActionResult> Check(int anuncioId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { isFavorito = false });
                }

                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

                if (comprador == null)
                {
                    return Json(new { isFavorito = false });
                }

                var isFavorito = await _context.AnunciosFavoritos
                    .AnyAsync(af => af.CompradorId == comprador.Id && af.AnuncioId == anuncioId);

                return Json(new { isFavorito = isFavorito });
            }
            catch
            {
                return Json(new { isFavorito = false });
            }
        }

        // GET: Favoritos/GetAll - Retorna IDs de todos os favoritos (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Json(new { success = false, favoritos = new int[] { } });
                }

                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

                if (comprador == null)
                {
                    return Json(new { success = false, favoritos = new int[] { } });
                }

                var favoritos = await _context.AnunciosFavoritos
                    .Where(af => af.CompradorId == comprador.Id)
                    .Select(af => af.AnuncioId)
                    .ToArrayAsync();

                return Json(new { success = true, favoritos = favoritos });
            }
            catch
            {
                return Json(new { success = false, favoritos = new int[] { } });
            }
        }

        // DELETE: Favoritos/Remove - Remove favorito (da página de favoritos)
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "Utilizador não autenticado";
                    return RedirectToAction("Login", "Utilizadores");
                }

                var comprador = await _context.Compradores
                    .FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);

                if (comprador == null)
                {
                    TempData["Error"] = "Comprador não encontrado";
                    return RedirectToAction("Index");
                }

                var favorito = await _context.AnunciosFavoritos
                    .FirstOrDefaultAsync(af => af.Id == id && af.CompradorId == comprador.Id);

                if (favorito == null)
                {
                    TempData["Error"] = "Favorito não encontrado";
                    return RedirectToAction("Index");
                }

                _context.AnunciosFavoritos.Remove(favorito);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Anúncio removido dos favoritos com sucesso!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao remover favorito: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
