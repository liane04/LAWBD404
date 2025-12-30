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

        // POST: Favoritos/RemoveMarca - Remove marca favorita (do perfil)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMarca(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Utilizadores");

            var utilizador = await _context.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);
            if (utilizador == null) return RedirectToAction("Perfil", "Utilizadores");

            var fav = await _context.MarcasFavoritas
                .FirstOrDefaultAsync(mf => mf.Id == id && mf.CompradorId == utilizador.Id);

            if (fav != null)
            {
                _context.MarcasFavoritas.Remove(fav);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Marca removida dos favoritos.";
            }

            return RedirectToAction("Perfil", "Utilizadores", null, "favoritos-marcas");
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
    
    

    // POST: Favoritos/ToggleMarca - Adiciona ou remove marca favorita (AJAX)
    [HttpPost]
    public async Task<IActionResult> ToggleMarca([FromBody] int marcaId)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { success = false, message = "Utilizador não autenticado" });

            // Tentar encontrar Utilizador associado (Comprador ou Vendedor)
            var utilizador = await _context.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);

            if (utilizador == null) return Json(new { success = false, message = "Perfil de utilizador não encontrado" });

            // Verifica se marca existe
            var marca = await _context.Marcas.FindAsync(marcaId);
            if (marca == null) return Json(new { success = false, message = "Marca não encontrada" });

            var fav = await _context.MarcasFavoritas
                .FirstOrDefaultAsync(mf => mf.CompradorId == utilizador.Id && mf.MarcaId == marcaId);

            bool adicionado;
            if (fav != null)
            {
                _context.MarcasFavoritas.Remove(fav);
                adicionado = false;
            }
            else
            {
                var novo = new MarcasFav
                {
                    CompradorId = utilizador.Id,
                    MarcaId = marcaId
                };
                _context.MarcasFavoritas.Add(novo);
                adicionado = true;
            }

            await _context.SaveChangesAsync();

            // Contar total
            var total = await _context.MarcasFavoritas.CountAsync(mf => mf.CompradorId == utilizador.Id);

            return Json(new { success = true, adicionado, total, message = adicionado ? "Marca adicionada aos favoritos" : "Marca removida dos favoritos" });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Erro: " + ex.Message });
        }
    }

    // GET: Favoritos/GetBrandsForSelection - Retorna todas as marcas e status de favorito
    [HttpGet]
    public async Task<IActionResult> GetBrandsForSelection()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { success = false, message = "Não autenticado" });

        var utilizador = await _context.Set<Utilizador>().FirstOrDefaultAsync(u => u.IdentityUserId == user.Id);
        if (utilizador == null) return Json(new { success = false, message = "Perfil não encontrado" });

        var allBrands = await _context.Marcas.OrderBy(m => m.Nome).ToListAsync();
        var myFavs = await _context.MarcasFavoritas
            .Where(mf => mf.CompradorId == utilizador.Id)
            .Select(mf => mf.MarcaId)
            .ToListAsync();

        var result = allBrands.Select(b => new
        {
            b.Id,
            b.Nome,
            IsFavorito = myFavs.Contains(b.Id)
        });

        return Json(new { success = true, brands = result });
    }
    }
}

