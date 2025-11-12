using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;

namespace Marketplace.Controllers
{
    public class UtilizadoresController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public UtilizadoresController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        // GET: Utilizadores
        public IActionResult Index() => View();

        // GET: Utilizadores/Perfil
        [Authorize]
        public IActionResult Perfil() => View();

        // GET: Utilizadores/Registar
        [HttpGet]
        public IActionResult Registar() => View();

        // POST: Utilizadores/Registar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(string nome, string email, string password, string confirmPassword, string userType)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["RegistarError"] = "Preencha todos os campos obrigatórios.";
                return View();
            }
            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                TempData["RegistarError"] = "As palavras-passe não coincidem.";
                return View();
            }
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                TempData["RegistarError"] = "Já existe uma conta com este email.";
                return View();
            }

            string username = email.Split('@')[0];
            var appUser = new ApplicationUser { UserName = username, Email = email, FullName = nome };
            var createRes = await _userManager.CreateAsync(appUser, password);
            if (!createRes.Succeeded)
            {
                TempData["RegistarError"] = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return View();
            }

            string role = string.Equals(userType, "vendedor", StringComparison.OrdinalIgnoreCase) ? "Vendedor" : "Comprador";
            await _userManager.AddToRoleAsync(appUser, role);

            // Entidade de domínio associada
            if (role == "Vendedor")
            {
                _db.Vendedores.Add(new Vendedor
                {
                    Username = username,
                    Email = email,
                    Nome = nome,
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = role,
                    IdentityUserId = appUser.Id
                });
            }
            else
            {
                _db.Compradores.Add(new Comprador
                {
                    Username = username,
                    Email = email,
                    Nome = nome,
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = role,
                    IdentityUserId = appUser.Id
                });
            }
            await _db.SaveChangesAsync();

            // Email de confirmação
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var link = Url.Action("ConfirmarEmail", "Utilizadores", new { userId = appUser.Id, token }, Request.Scheme)!;
            await _emailSender.SendAsync(email, "Confirme o seu email", $"Clique para confirmar: <a href=\"{link}\">{link}</a>");

            TempData["RegistarSucesso"] = "Conta criada. Verifique o seu email para confirmar.";
            return RedirectToAction("Login");
        }

        // GET: Utilizadores/Login
        [HttpGet]
        public IActionResult Login() => View();

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
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["LoginError"] = "Email ou palavra-passe incorretos.";
                return View();
            }
            if (!user.EmailConfirmed)
            {
                TempData["LoginError"] = "Confirme o seu email antes de entrar.";
                return View();
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                TempData["LoginError"] = "Email ou palavra-passe incorretos.";
                return View();
            }

            if (await _userManager.IsInRoleAsync(user, "Administrador"))
                return RedirectToAction("Index", "Administrador");
            if (await _userManager.IsInRoleAsync(user, "Vendedor"))
                return RedirectToAction("Index", "Anuncios");
            return RedirectToAction("Index", "Home");
        }

        // POST: Utilizadores/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: Utilizadores/ConfirmarEmail
        [HttpGet]
        public async Task<IActionResult> ConfirmarEmail(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return RedirectToAction("Login");
            var res = await _userManager.ConfirmEmailAsync(user, token);
            TempData[res.Succeeded ? "LoginInfo" : "LoginError"] = res.Succeeded
                ? "Email confirmado. Já pode iniciar sessão."
                : "Token inválido ou expirado.";
            return RedirectToAction("Login");
        }

        // POST: Utilizadores/ReenviarConfirmacao
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReenviarConfirmacao(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData["LoginError"] = "Conta não encontrada.";
                return RedirectToAction("Login");
            }
            if (user.EmailConfirmed)
            {
                TempData["LoginInfo"] = "Email já confirmado.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmarEmail", "Utilizadores", new { userId = user.Id, token }, Request.Scheme)!;
            await _emailSender.SendAsync(email, "Confirme o seu email", $"Clique para confirmar: <a href=\"{link}\">{link}</a>");
            TempData["LoginInfo"] = "Link de confirmação reenviado.";
            return RedirectToAction("Login");
        }
    }
}

