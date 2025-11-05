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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Marketplace.Services;

namespace Marketplace.Controllers
{
    public class UtilizadoresController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UtilizadoresController(ApplicationDbContext db)
        {
            _db = db;
        }

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

        // POST: Utilizadores/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Credenciais inválidas.";
                return View();
            }

            var user = await _db.Set<Utilizador>()
                .FirstOrDefaultAsync(u => u.Email == email);

            if (user == null || !PasswordHasher.VerifyPassword(password, user.PasswordHash))
            {
                TempData["LoginError"] = "Email ou palavra-passe incorretos.";
                return View();
            }

            string role = user switch
            {
                Administrador => "Administrador",
                Vendedor => "Vendedor",
                Comprador => "Comprador",
                _ => "Utilizador"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

            // Redirect by role
            return role switch
            {
                "Administrador" => RedirectToAction("Index", "Administrador"),
                "Vendedor" => RedirectToAction("Index", "Anuncios"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
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

