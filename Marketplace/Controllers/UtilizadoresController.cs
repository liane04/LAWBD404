using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
        private readonly IWebHostEnvironment _env;

        public UtilizadoresController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _env = env;
        }

        // GET: Utilizadores
        public IActionResult Index() => View();

        // GET: Utilizadores/Perfil
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            // Se o usuário for vendedor, carregar seus anúncios
            if (User.IsInRole("Vendedor"))
            {
                var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
                var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);

                if (vendedor != null)
                {
                    var anuncios = await _db.Anuncios
                        .Include(a => a.Marca)
                        .Include(a => a.Modelo)
                        .Include(a => a.Tipo)
                        .Include(a => a.Categoria)
                        .Include(a => a.Combustivel)
                        .Include(a => a.Imagens)
                        .Where(a => a.VendedorId == vendedor.Id)
                        .OrderByDescending(a => a.Id)
                        .ToListAsync();

                    ViewBag.MeusAnuncios = anuncios;
                    ViewBag.AnunciosCount = anuncios.Count;

                    var reservasCount = await _db.Reservas
                        .Include(r => r.Anuncio)
                        .Where(r => r.Anuncio.VendedorId == vendedor.Id)
                        .CountAsync();
                    ViewBag.ReservasRecebidasCount = reservasCount;

                    var visitasCount = await _db.Visitas
                        .Where(v => v.VendedorId == vendedor.Id)
                        .CountAsync();
                    ViewBag.VisitasAgendadasCount = visitasCount;

                    ViewBag.Nome = vendedor.Nome;
                    ViewBag.ImagemPerfil = string.IsNullOrWhiteSpace(vendedor.ImagemPerfil) ? null : vendedor.ImagemPerfil;
                }
            }

            return View();
        }

        // GET: Utilizadores/Edit
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null) return RedirectToAction("Login");

            var model = new Marketplace.Models.ViewModels.EditarPerfilViewModel
            {
                Nome = appUser.FullName ?? appUser.UserName ?? string.Empty,
                Email = appUser.Email,
                IsVendedor = await _userManager.IsInRoleAsync(appUser, "Vendedor")
            };

            if (model.IsVendedor)
            {
                var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    model.Nome = vendedor.Nome;
                    model.Nif = vendedor.Nif;
                    model.DadosFaturacao = vendedor.DadosFaturacao;
                    model.ImagemPerfilAtual = vendedor.ImagemPerfil;
                }
            }
            else
            {
                var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    model.Nome = comprador.Nome;
                    model.ImagemPerfilAtual = comprador.ImagemPerfil;
                }
            }

            return View(model);
        }

        // POST: Utilizadores/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Marketplace.Models.ViewModels.EditarPerfilViewModel model, IFormFile? fotoPerfil)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            if (appUser == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            appUser.FullName = model.Nome;
            await _userManager.UpdateAsync(appUser);

            if (await _userManager.IsInRoleAsync(appUser, "Vendedor"))
            {
                var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    vendedor.Nome = model.Nome;
                    vendedor.Nif = model.Nif;
                    vendedor.DadosFaturacao = model.DadosFaturacao;

                    if (fotoPerfil != null)
                    {
                        if (ImageUploadHelper.IsValidProfileImage(fotoPerfil, out var error))
                        {
                            var newPath = await ImageUploadHelper.UploadProfileImage(fotoPerfil, _env.WebRootPath, appUser.Id);
                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                if (!string.IsNullOrWhiteSpace(vendedor.ImagemPerfil))
                                {
                                    ImageUploadHelper.DeleteProfileImage(vendedor.ImagemPerfil, _env.WebRootPath);
                                }
                                vendedor.ImagemPerfil = newPath;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Falha ao guardar a imagem de perfil.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, error);
                            return View(model);
                        }
                    }
                }
            }
            else
            {
                var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    comprador.Nome = model.Nome;
                    if (fotoPerfil != null)
                    {
                        if (ImageUploadHelper.IsValidProfileImage(fotoPerfil, out var error))
                        {
                            var newPath = await ImageUploadHelper.UploadProfileImage(fotoPerfil, _env.WebRootPath, appUser.Id);
                            if (!string.IsNullOrWhiteSpace(newPath))
                            {
                                if (!string.IsNullOrWhiteSpace(comprador.ImagemPerfil))
                                {
                                    ImageUploadHelper.DeleteProfileImage(comprador.ImagemPerfil, _env.WebRootPath);
                                }
                                comprador.ImagemPerfil = newPath;
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Falha ao guardar a imagem de perfil.");
                                return View(model);
                            }
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, error);
                            return View(model);
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();
            TempData["PerfilSucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToAction("Perfil");
        }

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

            // Email de confirmação - OPCIONAL (não falhar se SMTP não estiver configurado)
            bool emailEnviado = false;
            try
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var link = Url.Action("ConfirmarEmail", "Utilizadores", new { userId = appUser.Id, token }, Request.Scheme)!;
                await _emailSender.SendAsync(email, "Confirme o seu email", $"Clique para confirmar: <a href=\"{link}\">{link}</a>");
                emailEnviado = true;
            }
            catch (Exception ex)
            {
                // Log do erro mas não falhar o registo
                Console.WriteLine($"⚠️  Erro ao enviar email de confirmação: {ex.Message}");
            }

            // Confirmar email automaticamente se o envio falhou (para desenvolvimento)
            if (!emailEnviado && !appUser.EmailConfirmed)
            {
                appUser.EmailConfirmed = true;
                await _userManager.UpdateAsync(appUser);
            }

            TempData["RegistarSucesso"] = emailEnviado
                ? "Conta criada. Verifique o seu email para confirmar."
                : "Conta criada com sucesso! Pode agora fazer login.";
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

            // NOTA: EmailConfirmed não é verificado aqui porque RequireConfirmedEmail = false no Program.cs

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
            if (user == null)
            {
                ViewBag.Sucesso = false;
                ViewBag.Mensagem = "Utilizador não encontrado.";
                return View();
            }

            var res = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.Sucesso = res.Succeeded;
            ViewBag.Mensagem = res.Succeeded
                ? "Email confirmado com sucesso! Pode agora fazer login na sua conta."
                : "Token inválido ou expirado. Por favor, solicite um novo email de confirmação.";
            ViewBag.Email = user.Email;

            return View();
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

        // GET: Utilizadores/EsqueceuPassword
        [HttpGet]
        public IActionResult EsqueceuPassword() => View();

        // POST: Utilizadores/EsqueceuPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EsqueceuPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewBag.Erro = "Por favor, insira o seu email.";
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Por segurança, não revelar se o email existe ou não
                ViewBag.Sucesso = true;
                ViewBag.Email = email;
                return View();
            }

            // Gerar token de recuperação de password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var link = Url.Action("RedefinirPassword", "Utilizadores", new { userId = user.Id, token }, Request.Scheme)!;

            // Tentar enviar email (opcional - não falhar se SMTP não configurado)
            bool emailEnviado = false;
            try
            {
                await _emailSender.SendAsync(
                    email,
                    "Recuperação de Palavra-passe - DriveDeal",
                    $@"<h2>Recuperação de Palavra-passe</h2>
                       <p>Recebemos um pedido para redefinir a palavra-passe da sua conta.</p>
                       <p>Clique no link abaixo para definir uma nova palavra-passe:</p>
                       <p><a href=""{link}"" style=""background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;"">Redefinir Palavra-passe</a></p>
                       <p>Ou copie e cole este link no seu navegador:</p>
                       <p>{link}</p>
                       <p><small>Se não solicitou esta alteração, ignore este email.</small></p>");
                emailEnviado = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erro ao enviar email de recuperação: {ex.Message}");
            }

            ViewBag.Sucesso = true;
            ViewBag.Email = email;
            ViewBag.EmailEnviado = emailEnviado;
            ViewBag.LinkRecuperacao = emailEnviado ? null : link; // Mostrar link se email não foi enviado (dev)
            return View();
        }

        // GET: Utilizadores/RedefinirPassword
        [HttpGet]
        public async Task<IActionResult> RedefinirPassword(int userId, string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            ViewBag.UserId = userId;
            ViewBag.Token = token;
            ViewBag.Email = user.Email;
            return View();
        }

        // POST: Utilizadores/RedefinirPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RedefinirPassword(int userId, string token, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Erro = "Por favor, preencha todos os campos.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
            {
                ViewBag.Erro = "As palavras-passe não coincidem.";
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                return View();
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                ViewBag.Erro = "Utilizador não encontrado.";
                return View();
            }

            var result = await _userManager.ResetPasswordAsync(user, token, password);
            if (!result.Succeeded)
            {
                ViewBag.Erro = string.Join("; ", result.Errors.Select(e => e.Description));
                ViewBag.UserId = userId;
                ViewBag.Token = token;
                ViewBag.Email = user.Email;
                return View();
            }

            ViewBag.Sucesso = true;
            ViewBag.Email = user.Email;
            return View();
        }
    }
}

