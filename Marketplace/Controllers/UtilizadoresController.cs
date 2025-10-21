using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marketplace.Controllers
{
    public class UtilizadoresController : Controller
    {

        /*
        private readonly MarketplaceContext _context;

        public UtilizadoresController(MarketplaceContext context)
        {
            _context = context;
        }
        */

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            /*
            var marketplaceContext = _context.Utilizador.Include(u => u.Morada);
            return View(await marketplaceContext.ToListAsync());
            */
            return View();
        }

        // GET: Utilizadores/Perfil
        public IActionResult Perfil()
        {
            return View();
        }

        // GET: Utilizadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            /*
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizador
                .Include(u => u.Morada)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utilizador == null)
            {
                return NotFound();
            }

            return View(utilizador);
            */
            return View();
        }

        // GET: Utilizadores/Registar
        public IActionResult Registar()
        {
            /*
            ViewData["MoradaId"] = new SelectList(_context.Set<Morada>(), "Id", "CodigoPostal");
            return View();
            */
            return View();
        }

        // GET: Utilizadores/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Utilizadores/Registar
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar([Bind("Id,Username,Email,Nome,PasswordHash,Estado,Tipo,MoradaId")] Utilizador utilizador)
        {
            /*
            if (ModelState.IsValid)
            {
                _context.Add(utilizador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MoradaId"] = new SelectList(_context.Set<Morada>(), "Id", "CodigoPostal", utilizador.MoradaId);
            return View(utilizador);
            */
            return View();
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            /*
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizador.FindAsync(id);
            if (utilizador == null)
            {
                return NotFound();
            }
            ViewData["MoradaId"] = new SelectList(_context.Set<Morada>(), "Id", "CodigoPostal", utilizador.MoradaId);
            return View(utilizador);
            */
            return View();
        }

        // POST: Utilizadores/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Username,Email,Nome,PasswordHash,Estado,Tipo,MoradaId")] Utilizador utilizador)
        {
            /*
            if (id != utilizador.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(utilizador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UtilizadorExists(utilizador.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MoradaId"] = new SelectList(_context.Set<Morada>(), "Id", "CodigoPostal", utilizador.MoradaId);
            return View(utilizador);
            */
            return View();
        }

        // GET: Utilizadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            /*
            if (id == null)
            {
                return NotFound();
            }

            var utilizador = await _context.Utilizador
                .Include(u => u.Morada)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (utilizador == null)
            {
                return NotFound();
            }

            return View(utilizador);
            */
            return View();
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            /*
            var utilizador = await _context.Utilizador.FindAsync(id);
            if (utilizador != null)
            {
                _context.Utilizador.Remove(utilizador);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
            */
            return View();
        }

        /*
        private bool UtilizadorExists(int id)
        {
            return _context.Utilizador.Any(e => e.Id == id);
        }
        */
    }
}

