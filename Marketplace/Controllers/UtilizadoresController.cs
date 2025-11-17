using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;

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
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Login");
            }

            // Tentar obter a entidade de domínio associada ao utilizador autenticado
            Utilizador? domainUser = await _db.Compradores
                .Include(c => c.Morada)
                .FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);

            if (domainUser == null)
            {
                domainUser = await _db.Vendedores
                    .Include(v => v.Morada)
                    .FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
            }

            if (domainUser == null)
            {
                domainUser = await _db.Administradores
                    .Include(a => a.Morada)
                    .FirstOrDefaultAsync(a => a.IdentityUserId == appUser.Id);
            }

            var roles = await _userManager.GetRolesAsync(appUser);
            var claims = await _userManager.GetClaimsAsync(appUser);
            bool sellerPending = claims.Any(c => c.Type == "SellerRequest" && c.Value == "Pendente");

            var vm = new ProfileViewModel
            {
                FullName = appUser.FullName ?? domainUser?.Nome ?? appUser.UserName ?? "Utilizador",
                Email = appUser.Email ?? domainUser?.Email ?? string.Empty,
                Username = appUser.UserName ?? domainUser?.Username ?? string.Empty,
                Estado = domainUser?.Estado ?? "Ativo",
                ImagemPerfilUrl = appUser.ImagemPerfil ?? domainUser?.ImagemPerfil,
                MoradaRua = domainUser?.Morada?.Rua,
                MoradaCodigoPostal = domainUser?.Morada?.CodigoPostal,
                MoradaLocalidade = domainUser?.Morada?.Localidade,
                Roles = roles
                , SellerRequestPending = sellerPending
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PedirVendedor()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Login");
            if (await _userManager.IsInRoleAsync(appUser, "Vendedor"))
            {
                TempData["PerfilInfo"] = "Já possui permissões de vendedor.";
                return RedirectToAction(nameof(Perfil));
            }

            var claims = await _userManager.GetClaimsAsync(appUser);
            if (claims.Any(c => c.Type == "SellerRequest" && c.Value == "Pendente"))
            {
                TempData["PerfilInfo"] = "O seu pedido para vendedor já está pendente.";
                return RedirectToAction(nameof(Perfil));
            }

            await _userManager.AddClaimAsync(appUser, new System.Security.Claims.Claim("SellerRequest", "Pendente"));
            TempData["PerfilSucesso"] = "Pedido para vendedor submetido. Aguarde validação do administrador.";
            return RedirectToAction(nameof(Perfil));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelarPedidoVendedor()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Login");
            var claims = await _userManager.GetClaimsAsync(appUser);
            var pending = claims.FirstOrDefault(c => c.Type == "SellerRequest" && c.Value == "Pendente");
            if (pending != null)
            {
                await _userManager.RemoveClaimAsync(appUser, pending);
                TempData["PerfilSucesso"] = "Pedido de vendedor cancelado.";
            }
            else
            {
                TempData["PerfilInfo"] = "Não existe pedido pendente.";
            }
            return RedirectToAction(nameof(Perfil));
        }

        // GET: Utilizadores/Edit
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Login");

            Utilizador? domainUser = await _db.Compradores
                .Include(c => c.Morada)
                .FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);

            if (domainUser == null)
            {
                domainUser = await _db.Vendedores
                    .Include(v => v.Morada)
                    .FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
            }

            if (domainUser == null)
            {
                domainUser = await _db.Administradores
                    .Include(a => a.Morada)
                    .FirstOrDefaultAsync(a => a.IdentityUserId == appUser.Id);
            }

            var vm = new EditProfileViewModel
            {
                FullName = appUser.FullName ?? domainUser?.Nome ?? string.Empty,
                Email = appUser.Email ?? domainUser?.Email ?? string.Empty,
                Username = appUser.UserName ?? domainUser?.Username ?? string.Empty,
                ImagemAtual = appUser.ImagemPerfil ?? domainUser?.ImagemPerfil,
                MoradaRua = domainUser?.Morada?.Rua,
                MoradaCodigoPostal = domainUser?.Morada?.CodigoPostal,
                MoradaLocalidade = domainUser?.Morada?.Localidade
            };

            return View(vm);
        }

        // POST: Utilizadores/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel vm)
        {
            var appUser = await _userManager.GetUserAsync(User);
            if (appUser == null) return RedirectToAction("Login");

            if (!ModelState.IsValid)
            {
                // Repreenche campos informativos
                vm.Email = appUser.Email ?? string.Empty;
                vm.Username = appUser.UserName ?? string.Empty;
                vm.ImagemAtual = appUser.ImagemPerfil;
                return View(vm);
            }

            Utilizador? domainUser = await _db.Compradores
                .Include(c => c.Morada)
                .FirstOrDefaultAsync(c => c.IdentityUserId == appUser.Id);

            if (domainUser == null)
            {
                domainUser = await _db.Vendedores
                    .Include(v => v.Morada)
                    .FirstOrDefaultAsync(v => v.IdentityUserId == appUser.Id);
            }

            if (domainUser == null)
            {
                domainUser = await _db.Administradores
                    .Include(a => a.Morada)
                    .FirstOrDefaultAsync(a => a.IdentityUserId == appUser.Id);
            }

            // Atualizar nome
            appUser.FullName = vm.FullName;
            if (domainUser != null)
            {
                domainUser.Nome = vm.FullName;
            }

            // Atualizar/gestionar imagem de perfil
            if (vm.RemoverImagem)
            {
                ImageUploadHelper.DeleteProfileImage(appUser.ImagemPerfil, _env.WebRootPath);
                appUser.ImagemPerfil = null;
                if (domainUser != null) domainUser.ImagemPerfil = null;
            }
            else if (vm.ProfileImage != null)
            {
                if (ImageUploadHelper.IsValidProfileImage(vm.ProfileImage, out var err))
                {
                    var newPath = await ImageUploadHelper.UploadProfileImage(vm.ProfileImage, _env.WebRootPath, appUser.Id);
                    if (!string.IsNullOrWhiteSpace(newPath))
                    {
                        ImageUploadHelper.DeleteProfileImage(appUser.ImagemPerfil, _env.WebRootPath);
                        appUser.ImagemPerfil = newPath;
                        if (domainUser != null) domainUser.ImagemPerfil = newPath;
                    }
                    else
                    {
                        ModelState.AddModelError("ProfileImage", "Falha ao carregar a imagem. Tente um ficheiro diferente.");
                        vm.Email = appUser.Email ?? string.Empty;
                        vm.Username = appUser.UserName ?? string.Empty;
                        vm.ImagemAtual = appUser.ImagemPerfil;
                        return View(vm);
                    }
                }
                else
                {
                    ModelState.AddModelError("ProfileImage", err);
                    vm.Email = appUser.Email ?? string.Empty;
                    vm.Username = appUser.UserName ?? string.Empty;
                    vm.ImagemAtual = appUser.ImagemPerfil;
                    return View(vm);
                }
            }

            // Atualizar Morada
            if (domainUser != null)
            {
                if (domainUser.Morada == null && (!string.IsNullOrWhiteSpace(vm.MoradaRua) || !string.IsNullOrWhiteSpace(vm.MoradaCodigoPostal) || !string.IsNullOrWhiteSpace(vm.MoradaLocalidade)))
                {
                    domainUser.Morada = new Morada
                    {
                        Rua = vm.MoradaRua ?? string.Empty,
                        CodigoPostal = vm.MoradaCodigoPostal ?? string.Empty,
                        Localidade = vm.MoradaLocalidade ?? string.Empty
                    };
                }
                else if (domainUser.Morada != null)
                {
                    domainUser.Morada.Rua = vm.MoradaRua ?? domainUser.Morada.Rua;
                    domainUser.Morada.CodigoPostal = vm.MoradaCodigoPostal ?? domainUser.Morada.CodigoPostal;
                    domainUser.Morada.Localidade = vm.MoradaLocalidade ?? domainUser.Morada.Localidade;
                }
            }

            // Persistir alterações
            var identityRes = await _userManager.UpdateAsync(appUser);
            if (!identityRes.Succeeded)
            {
                TempData["PerfilErro"] = string.Join("; ", identityRes.Errors.Select(e => e.Description));
                return View(vm);
            }

            await _db.SaveChangesAsync();
            TempData["PerfilSucesso"] = "Perfil atualizado com sucesso.";
            return RedirectToAction(nameof(Perfil));
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

            var posMsg = role == "Vendedor"
                ? "Conta criada com sucesso! Aguarde validação por um administrador."
                : "Conta criada com sucesso!";

            TempData["RegistarSucesso"] = emailEnviado
                ? (role == "Vendedor" ? "Conta criada. Verifique o email (se aplicável). Aguarde validação." : "Conta criada. Verifique o seu email para confirmar.")
                : posMsg;
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

