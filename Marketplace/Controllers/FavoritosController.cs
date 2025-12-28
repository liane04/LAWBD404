using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace.Controllers
{
    [Authorize] // Permite qualquer utilizador autenticado
    public class FavoritosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FavoritosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Favoritos - Lista todos os anúncios favoritos do utilizador
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Utilizadores");

            var utilizador = await _context.Set<Utilizador>()
                .Include(u => u.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Marca)
                .Include(u => u.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Modelo)
                .Include(u => u.AnunciosFavoritos)
                    .ThenInclude(af => af.Anuncio)
                        .ThenInclude(a => a.Imagens)
                // Se precisar de mais includes...
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (utilizador == null) return NotFound("Perfil de utilizador não encontrado");

            return View(utilizador.AnunciosFavoritos.OrderByDescending(af => af.Id).ToList());
        }

        // POST: Favoritos/Toggle - Adiciona ou remove favorito (AJAX)
        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] int anuncioId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Json(new { success = false, message = "Utilizador não autenticado" });

                var utilizador = await _context.Set<Utilizador>()
                    .Include(u => u.AnunciosFavoritos)
                    .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

                if (utilizador == null) return Json(new { success = false, message = "Utilizador não encontrado" });

                // Verifica se o anúncio existe
                var anuncio = await _context.Anuncios.FindAsync(anuncioId);
                if (anuncio == null) return Json(new { success = false, message = "Anúncio não encontrado" });

                // Verifica se já é favorito
                var favoritoExistente = utilizador.AnunciosFavoritos
                    .FirstOrDefault(af => af.AnuncioId == anuncioId);

                bool adicionado;

                if (favoritoExistente != null)
                {
                    _context.AnunciosFavoritos.Remove(favoritoExistente);
                    adicionado = false;
                }
                else
                {
                    var novoFavorito = new AnuncioFav
                    {
                        CompradorId = utilizador.Id, // Aqui usamos o ID do Utilizador (mesmo sendo propriedade CompradorId)
                        AnuncioId = anuncioId
                    };
                    _context.AnunciosFavoritos.Add(novoFavorito);
                    adicionado = true;
                }

                await _context.SaveChangesAsync();

                var totalFavoritos = await _context.AnunciosFavoritos
                    .CountAsync(af => af.CompradorId == utilizador.Id);

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
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { isFavorito = false });

            var utilizador = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (utilizador == null) return Json(new { isFavorito = false });

            var isFavorito = await _context.AnunciosFavoritos
                .AnyAsync(af => af.CompradorId == utilizador.Id && af.AnuncioId == anuncioId);

            return Json(new { isFavorito = isFavorito });
        }

        // GET: Favoritos/GetAll - Retorna IDs de todos os favoritos (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, favoritos = new int[] { } });

            var utilizador = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (utilizador == null) return Json(new { success = false, favoritos = new int[] { } });

            var favoritos = await _context.AnunciosFavoritos
                .Where(af => af.CompradorId == utilizador.Id)
                .Select(af => af.AnuncioId)
                .ToArrayAsync();

            return Json(new { success = true, favoritos = favoritos });
        }

        // DELETE: Favoritos/Remove - Remove favorito (da página de favoritos)
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Utilizadores");

            var utilizador = await _context.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (utilizador == null) return RedirectToAction("Index");

            var favorito = await _context.AnunciosFavoritos
                .FirstOrDefaultAsync(af => af.Id == id && af.CompradorId == utilizador.Id);

            if (favorito != null)
            {
                _context.AnunciosFavoritos.Remove(favorito);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Removido dos favoritos.";
            }

            return RedirectToAction("Index");
        }
    }
}

