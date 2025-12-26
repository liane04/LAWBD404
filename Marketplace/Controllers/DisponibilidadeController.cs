using Marketplace.Data;
using Marketplace.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Controllers
{
    [Authorize(Roles = "Vendedor")]
    public class DisponibilidadeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DisponibilidadeController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Disponibilidade
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null)
                return Forbid();

            var disponibilidades = await _context.DisponibilidadesVendedor
                .Where(d => d.VendedorId == vendedor.Id)
                .OrderBy(d => d.DiaSemana)
                .ThenBy(d => d.HoraInicio)
                .ToListAsync();

            return View(disponibilidades);
        }

        // GET: Disponibilidade/Create
        public IActionResult Create()
        {
            // Limpar ModelState para evitar problemas com checkbox
            ModelState.Clear();
            return View(new DisponibilidadeVendedor());
        }

        // POST: Disponibilidade/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DiaSemana,HoraInicio,HoraFim,IntervaloMinutos,Ativo")] DisponibilidadeVendedor disponibilidade)
        {
            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null)
                return Forbid();

            // Remover validação da propriedade de navegação
            ModelState.Remove("Vendedor");

            // Validações
            if (disponibilidade.HoraFim <= disponibilidade.HoraInicio)
            {
                ModelState.AddModelError("HoraFim", "A hora de fim deve ser posterior à hora de início");
            }

            if (ModelState.IsValid)
            {
                disponibilidade.VendedorId = vendedor.Id;
                disponibilidade.DataCriacao = DateTime.Now;

                _context.Add(disponibilidade);
                await _context.SaveChangesAsync();

                TempData["PerfilSucesso"] = "Disponibilidade adicionada com sucesso!";
                return Redirect("/Utilizadores/Perfil#disponibilidade");
            }

            return View(disponibilidade);
        }

        // GET: Disponibilidade/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null)
                return Forbid();

            var disponibilidade = await _context.DisponibilidadesVendedor
                .FirstOrDefaultAsync(d => d.Id == id && d.VendedorId == vendedor.Id);

            if (disponibilidade == null)
                return NotFound();

            // Limpar ModelState para evitar problemas com checkbox
            ModelState.Clear();
            return View(disponibilidade);
        }

        // POST: Disponibilidade/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DiaSemana,HoraInicio,HoraFim,IntervaloMinutos,Ativo,VendedorId,DataCriacao")] DisponibilidadeVendedor disponibilidade)
        {
            if (id != disponibilidade.Id)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null || disponibilidade.VendedorId != vendedor.Id)
                return Forbid();

            // Remover validação da propriedade de navegação
            ModelState.Remove("Vendedor");

            // Validações
            if (disponibilidade.HoraFim <= disponibilidade.HoraInicio)
            {
                ModelState.AddModelError("HoraFim", "A hora de fim deve ser posterior à hora de início");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(disponibilidade);
                    await _context.SaveChangesAsync();
                    TempData["PerfilSucesso"] = "Disponibilidade atualizada com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DisponibilidadeExists(disponibilidade.Id))
                        return NotFound();
                    else
                        throw;
                }
                return Redirect("/Utilizadores/Perfil#disponibilidade");
            }

            return View(disponibilidade);
        }

        // POST: Disponibilidade/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);

            if (vendedor == null)
                return Forbid();

            var disponibilidade = await _context.DisponibilidadesVendedor
                .FirstOrDefaultAsync(d => d.Id == id && d.VendedorId == vendedor.Id);

            if (disponibilidade == null)
                return NotFound();

            _context.DisponibilidadesVendedor.Remove(disponibilidade);
            await _context.SaveChangesAsync();

            TempData["PerfilSucesso"] = "Disponibilidade removida com sucesso!";
            return Redirect("/Utilizadores/Perfil#disponibilidade");
        }

        private bool DisponibilidadeExists(int id)
        {
            return _context.DisponibilidadesVendedor.Any(e => e.Id == id);
        }
    }
}
