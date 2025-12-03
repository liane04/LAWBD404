using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
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
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return Json(new { available = false });
            var existing = await _userManager.FindByNameAsync(username);
            return Json(new { available = existing == null });
        }

        // GET: Utilizadores/Perfil
        [Authorize]
        public async Task<IActionResult> Perfil()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Se o usuário for vendedor, carregar seus anúncios
            if (User.IsInRole("Vendedor"))
            {
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

                    var visitasVendedor = await _db.Visitas
                        .Include(v => v.Comprador)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Where(v => v.VendedorId == vendedor.Id)
                        .OrderByDescending(v => v.Data)
                        .ToListAsync();

                    ViewBag.MinhasVisitasVendedor = visitasVendedor;
                    ViewBag.VisitasAgendadasCount = visitasVendedor.Count;

                    ViewBag.Nome = vendedor.Nome;
                    ViewBag.ImagemPerfil = string.IsNullOrWhiteSpace(vendedor.ImagemPerfil) ? null : vendedor.ImagemPerfil;
                    ViewBag.VendedorEstado = vendedor.Estado;
                }
            }
            // Se o usuário for comprador, carregar seus favoritos
            else if (User.IsInRole("Comprador"))
            {
                var comprador = await _db.Compradores
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
                            .ThenInclude(a => a.Tipo)
                    .FirstOrDefaultAsync(c => c.IdentityUserId == userId);

                if (comprador != null)
                {
                    ViewBag.MeusFavoritos = comprador.AnunciosFavoritos.OrderByDescending(af => af.Id).ToList();
                    ViewBag.FavoritosCount = comprador.AnunciosFavoritos.Count;

                    // Carregar visitas do comprador
                    var visitasComprador = await _db.Visitas
                        .Include(v => v.Vendedor)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Marca)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Modelo)
                        .Include(v => v.Anuncio)
                            .ThenInclude(a => a.Imagens)
                        .Where(v => v.CompradorId == comprador.Id)
                        .OrderByDescending(v => v.Data)
                        .ToListAsync();

                    ViewBag.MinhasVisitasComprador = visitasComprador;

                    ViewBag.Nome = comprador.Nome;
                    ViewBag.ImagemPerfil = string.IsNullOrWhiteSpace(comprador.ImagemPerfil) ? null : comprador.ImagemPerfil;
                }
            }

            return View();
        }

        // GET: Utilizadores/PerfilDados (JSON para dinamizar a view de Perfil)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PerfilDados()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var appUser = await _userManager.FindByIdAsync(userId.ToString());
            string? nome = appUser?.FullName ?? appUser?.UserName;
            string? email = appUser?.Email;
            string? phone = appUser?.PhoneNumber;
            string? imagem = null;
            string? rua = null;
            string? localidade = null;
            string? codigoPostal = null;

            if (User.IsInRole("Vendedor"))
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == userId);
                if (vendedor != null)
                {
                    nome = vendedor.Nome;
                    imagem = vendedor.ImagemPerfil;
                    if (vendedor.Morada != null)
                    {
                        rua = vendedor.Morada.Rua;
                        localidade = vendedor.Morada.Localidade;
                        codigoPostal = vendedor.Morada.CodigoPostal;
                    }
                }
            }
            else if (User.IsInRole("Comprador"))
            {
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == userId);
                if (comprador != null)
                {
                    nome = comprador.Nome;
                    imagem = comprador.ImagemPerfil;
                    if (comprador.Morada != null)
                    {
                        rua = comprador.Morada.Rua;
                        localidade = comprador.Morada.Localidade;
                        codigoPostal = comprador.Morada.CodigoPostal;
                    }
                }
            }

            return Json(new { nome, email, phone, imagemPerfil = imagem, morada = new { rua, localidade, codigoPostal } });
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
                Telefone = appUser.PhoneNumber,
                IsVendedor = await _userManager.IsInRoleAsync(appUser, "Vendedor")
            };

            if (model.IsVendedor)
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    model.Nome = vendedor.Nome;
                    model.Nif = vendedor.Nif;
                    model.DadosFaturacao = vendedor.DadosFaturacao;
                    model.ImagemPerfilAtual = vendedor.ImagemPerfil;
                    if (vendedor.Morada != null)
                    {
                        model.Rua = vendedor.Morada.Rua;
                        model.Localidade = vendedor.Morada.Localidade;
                        model.CodigoPostal = vendedor.Morada.CodigoPostal;
                    }
                }
            }
            else
            {
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    model.Nome = comprador.Nome;
                    model.ImagemPerfilAtual = comprador.ImagemPerfil;
                    if (comprador.Morada != null)
                    {
                        model.Rua = comprador.Morada.Rua;
                        model.Localidade = comprador.Morada.Localidade;
                        model.CodigoPostal = comprador.Morada.CodigoPostal;
                    }
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

            // Validação extra: NIF (checksum PT)
            if (!string.IsNullOrWhiteSpace(model.Nif) && !IsValidNif(model.Nif))
            {
                ModelState.AddModelError("Nif", "NIF inválido (checksum)");
                return View(model);
            }

            appUser.FullName = model.Nome;
            if (!string.IsNullOrWhiteSpace(model.Telefone))
            {
                appUser.PhoneNumber = model.Telefone;
            }
            await _userManager.UpdateAsync(appUser);

            if (await _userManager.IsInRoleAsync(appUser, "Vendedor"))
            {
                var vendedor = await _db.Vendedores.Include(v => v.Morada).FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
                if (vendedor != null)
                {
                    vendedor.Nome = model.Nome;
                    vendedor.Nif = model.Nif;
                    vendedor.DadosFaturacao = model.DadosFaturacao;

                    // Morada
                    if (!string.IsNullOrWhiteSpace(model.Rua) && !string.IsNullOrWhiteSpace(model.Localidade) && !string.IsNullOrWhiteSpace(model.CodigoPostal))
                    {
                        if (vendedor.Morada == null)
                        {
                            vendedor.Morada = new Morada
                            {
                                Rua = model.Rua!,
                                Localidade = model.Localidade!,
                                CodigoPostal = model.CodigoPostal!
                            };
                        }
                        else
                        {
                            vendedor.Morada.Rua = model.Rua!;
                            vendedor.Morada.Localidade = model.Localidade!;
                            vendedor.Morada.CodigoPostal = model.CodigoPostal!;
                        }
                    }

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
                var comprador = await _db.Compradores.Include(c => c.Morada).FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);
                if (comprador != null)
                {
                    comprador.Nome = model.Nome;
                    // Morada
                    if (!string.IsNullOrWhiteSpace(model.Rua) && !string.IsNullOrWhiteSpace(model.Localidade) && !string.IsNullOrWhiteSpace(model.CodigoPostal))
                    {
                        if (comprador.Morada == null)
                        {
                            comprador.Morada = new Morada
                            {
                                Rua = model.Rua!,
                                Localidade = model.Localidade!,
                                CodigoPostal = model.CodigoPostal!
                            };
                        }
                        else
                        {
                            comprador.Morada.Rua = model.Rua!;
                            comprador.Morada.Localidade = model.Localidade!;
                            comprador.Morada.CodigoPostal = model.CodigoPostal!;
                        }
                    }
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

        private bool IsValidNif(string? nif)
        {
            if (string.IsNullOrWhiteSpace(nif)) return true;
            var digits = new string(nif.Where(char.IsDigit).ToArray());
            if (digits.Length != 9) return false;
            var first = digits[0];
            if ("1235689".IndexOf(first) < 0) return false;
            int sum = 0;
            for (int i = 0; i < 8; i++)
            {
                sum += (digits[i] - '0') * (9 - i);
            }
            var mod11 = sum % 11;
            var check = 11 - mod11;
            if (check >= 10) check = 0;
            return check == (digits[8] - '0');
        }

        // GET: Utilizadores/Registar
        [HttpGet]
        public IActionResult Registar() => View();

        // POST: Utilizadores/Registar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registar(string nome, string email, string username, string password, string confirmPassword, string userType)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
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
                    Estado = "Pendente",
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
                var html = Marketplace.Services.EmailTemplates.ConfirmEmail("DriveDeal", link);
                await _emailSender.SendAsync(email, "Confirmação de Email - DriveDeal", html);
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
        public async Task<IActionResult> Login(string identifier, string password, bool rememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                TempData["LoginError"] = "Credenciais inválidas.";
                return View();
            }
            var user = await _userManager.FindByEmailAsync(identifier);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(identifier);
            }
            if (user == null)
            {
                TempData["LoginError"] = "Credenciais incorretas.";
                return View();
            }

            // NOTA: EmailConfirmed não é verificado aqui porque RequireConfirmedEmail = false no Program.cs

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, password, rememberMe, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                var lockoutEnd = user.LockoutEnd.HasValue ? user.LockoutEnd.Value.LocalDateTime.ToString("dd/MM/yyyy HH:mm") : "indefinidamente";
                
                TempData["LoginError"] = $"A sua conta encontra-se bloqueada até {lockoutEnd}.";
                return View();
            }

            if (!result.Succeeded)
            {
                TempData["LoginError"] = "Credenciais incorretas.";
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

        // POST: Utilizadores/ExternalLogin (Google)
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            if (string.IsNullOrWhiteSpace(provider))
                return RedirectToAction(nameof(Login));

            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Utilizadores", new { returnUrl });
            var props = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(props, provider);
        }

        // GET: Utilizadores/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError))
            {
                TempData["LoginError"] = $"Falha no login externo: {remoteError}";
                return RedirectToAction(nameof(Login));
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["LoginError"] = "Não foi possível obter informações do login externo.";
                return RedirectToAction(nameof(Login));
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl ?? "/");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email ?? "Utilizador";

            if (email == null)
            {
                TempData["LoginError"] = "A conta externa não forneceu um email válido.";
                return RedirectToAction(nameof(Login));
            }

            // Se já existe conta com este email, associar o login externo
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                try { await _userManager.AddLoginAsync(existingUser, info); } catch { /* ignorar se já associado */ }
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
                return LocalRedirect(returnUrl ?? "/");
            }

            // Criar nova conta e entidade de domínio (Comprador)
            var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var createRes = await _userManager.CreateAsync(user);
            if (!createRes.Succeeded)
            {
                TempData["LoginError"] = string.Join("; ", createRes.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }

            await _userManager.AddLoginAsync(user, info);
            await _userManager.AddToRoleAsync(user, "Comprador");

            var comprador = new Comprador
            {
                Username = user.UserName!,
                Email = user.Email!,
                Nome = name,
                IdentityUserId = user.Id,
                PasswordHash = "IDENTITY"
            };
            _db.Compradores.Add(comprador);
            await _db.SaveChangesAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? "/");
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
                return View("ConfirmarEmailNew");
            }

            var res = await _userManager.ConfirmEmailAsync(user, token);
            ViewBag.Sucesso = res.Succeeded;
            ViewBag.Mensagem = res.Succeeded
                ? "Email confirmado com sucesso! Pode agora fazer login na sua conta."
                : "Token inválido ou expirado. Por favor, solicite um novo email de confirmação.";
            ViewBag.Email = user.Email;

            return View("ConfirmarEmailNew");
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
            var html = Marketplace.Services.EmailTemplates.ConfirmEmail("DriveDeal", link);
            await _emailSender.SendAsync(email, "Confirmação de Email - DriveDeal", html);
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
        public async Task<IActionResult> PromoteMe()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == userId);
            if (vendedor != null)
            {
                vendedor.Estado = "Ativo";
                await _db.SaveChangesAsync();
                return Content("Promoted!");
            }
            return Content("Not found");
        }
    }
}




