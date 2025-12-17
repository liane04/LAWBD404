using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Marketplace.Data;
using Marketplace.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace Marketplace.Controllers
{

    [Authorize(Roles = "Administrador")]
    public class AdministradorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Marketplace.Services.IEmailSender? _emailSender;

        public AdministradorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, Marketplace.Services.IEmailSender? emailSender)
        {
            _db = db;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Index(string? section = null, string? historyFilter = null)
        {
            // Auto-fix data issues transparently
            try 
            {
                await _db.Database.ExecuteSqlRawAsync("UPDATE HistoricoAcao SET TipoAcao = 'AcaoUser' WHERE TipoAcao IN ('Aprovar', 'Rejeitar', 'Bloquear', 'Desbloquear')");
                await _db.Database.ExecuteSqlRawAsync("UPDATE HistoricoAcao SET TipoAcao = 'AcaoAnuncio' WHERE TipoAcao IN ('Pausar', 'Retomar', 'Anúncio Pausado', 'Anúncio Retomado')");
                await _db.Database.ExecuteSqlRawAsync("UPDATE Administradores SET NivelAcesso = 'Nivel 1' WHERE NivelAcesso = 'Total'");
            }
            catch 
            {
                // Ignore errors during auto-fix to prevent blocking the dashboard
            }

            ViewBag.ActiveSection = section;
            ViewBag.HistoryFilter = historyFilter;

            var currentAdmin = await GetCurrentAdminAsync();
            ViewBag.NivelAcesso = currentAdmin?.NivelAcesso ?? "Nivel 2"; 
            ViewBag.AdminName = currentAdmin?.Nome ?? "Administrador";
            ViewBag.AdminEmail = currentAdmin?.Email ?? "admin@marketplace404.pt";
            ViewBag.AdminFoto = currentAdmin?.ImagemPerfil;

            // Calculate Dashboard Stats
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Top 5 Marcas Mais Vendidas
            var topMarcas = await _db.Compras
                .Include(c => c.Anuncio)
                .ThenInclude(a => a.Marca)
                .Where(c => c.Anuncio.Marca != null)
                .GroupBy(c => c.Anuncio.Marca!.Nome)
                .Select(g => new TopMarcaVM
                {
                    Nome = g.Key,
                    Vendas = g.Count()
                })
                .OrderByDescending(m => m.Vendas)
                .Take(5)
                .ToListAsync();

            // Calculate percentages for progress bars
            var maxVendas = topMarcas.Any() ? topMarcas.Max(m => m.Vendas) : 1;
            foreach (var marca in topMarcas)
            {
                marca.Percentagem = (int)((double)marca.Vendas / maxVendas * 100);
            }

            var stats = new DashboardStatsVM
            {
                TotalCompradores = await _db.Compradores.CountAsync(),
                TotalVendedores = await _db.Vendedores.CountAsync(),
                TotalAnunciosAtivos = await _db.Anuncios.CountAsync(), 
                VendasMes = await _db.Compras.CountAsync(c => c.Data >= startOfMonth),
                VolumeVendasMes = await _db.Compras
                    .Where(c => c.Data >= startOfMonth)
                    .Include(c => c.Anuncio)
                    .SumAsync(c => (decimal?)c.Anuncio.Preco) ?? 0,
                
                VendedoresPendentes = await _db.Vendedores.CountAsync(v => v.Estado == "Pendente"),
                DenunciasAbertas = (await _db.DenunciasAnuncio.CountAsync(d => d.Estado == "Pendente" || d.Estado == "Em Análise")) +
                                   (await _db.DenunciasUser.CountAsync(d => d.Estado == "Pendente" || d.Estado == "Em Análise")),
                AnunciosPendentes = await _db.Anuncios.CountAsync(a => !a.AcoesAnuncio.Any()), // Ads with no actions are considered pending
                DestaquesAtivos = 0,   // Not implemented yet

                TopMarcas = topMarcas,
                TotalVisualizacoes = await _db.Anuncios.SumAsync(a => a.NVisualizacoes),
                TotalFavoritos = await _db.AnunciosFavoritos.CountAsync(),
                TotalReservasAtivas = await _db.Reservas.CountAsync(r => r.Estado == "Pendente" || r.Estado == "Aceite"),
                TotalMensagens = await _db.Mensagens.CountAsync()
            };

            return View(stats);
        }

        public class DashboardStatsVM
        {
            public int TotalCompradores { get; set; }
            public int TotalVendedores { get; set; }
            public int TotalAnunciosAtivos { get; set; }
            public int VendasMes { get; set; }
            public decimal VolumeVendasMes { get; set; }
            public int VendedoresPendentes { get; set; }
            public int DenunciasAbertas { get; set; }
            public int AnunciosPendentes { get; set; }
            public int DestaquesAtivos { get; set; }

            public List<TopMarcaVM> TopMarcas { get; set; } = new();
            public int TotalVisualizacoes { get; set; }
            public int TotalFavoritos { get; set; }
            public int TotalReservasAtivas { get; set; }
            public int TotalMensagens { get; set; }
        }

        public class TopMarcaVM
        {
            public string Nome { get; set; } = string.Empty;
            public int Vendas { get; set; }
            public int Percentagem { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> ValidarVendedores()
        {
            var pendentes = await _db.Vendedores
                .Where(v => v.Estado == null || v.Estado == "Pendente")
                .OrderBy(v => v.Nome)
                .ToListAsync();
            return View(pendentes);
        }

        private async Task<Administrador?> GetCurrentAdminAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;
            return await _db.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);
        }

        // POST: Administrador/AprovarVendedor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarVendedor(int id)
        {
            var vendedor = await _db.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            vendedor.Estado = "Ativo";

            // Registar ação
            var admin = await GetCurrentAdminAsync();
            if (admin != null)
            {
                var acao = new AcaoUser
                {
                    UtilizadorId = id,
                    Data = DateTime.UtcNow,
                    AdministradorId = admin.Id,
                    Motivo = "Aprovar: Vendedor Aprovado"
                };
                _db.Add(acao);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Vendedor '{vendedor.Nome}' aprovado com sucesso!";

            // Notificar vendedor por email
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(vendedor.Email))
                {
                    await _emailSender.SendAsync(
                        vendedor.Email,
                        "Aprovação de Vendedor - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #0d6efd;'>Parabéns! Seu pedido foi aprovado</h2>
                            <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                            <p>O seu pedido para se tornar vendedor na plataforma <strong>404 Ride</strong> foi <span style='color: #28a745;'>aprovado</span>.</p>
                            <p>Já pode começar a criar e gerir os seus anúncios de veículos!</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the approval
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            // Registar ação
            // admin já foi definido anteriormente
            if (admin != null)
            {
                var acao = new AcaoUser
                {
                    UtilizadorId = id,
                    Data = DateTime.UtcNow,
                    AdministradorId = admin.Id,
                    Motivo = "Rejeitar: Vendedor Rejeitado"
                };
                _db.Add(acao);
            }

            await _db.SaveChangesAsync();

            TempData["Warning"] = $"Vendedor '{vendedor.Nome}' foi rejeitado.";

            // Notificar vendedor por email
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(vendedor.Email))
                {
                    await _emailSender.SendAsync(
                        vendedor.Email,
                        "Pedido de Vendedor - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #dc3545;'>Pedido não aprovado</h2>
                            <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                            <p>Lamentamos informar que o seu pedido para se tornar vendedor na plataforma <strong>404 Ride</strong> não foi aprovado neste momento.</p>
                            <p>Se necessitar de esclarecimentos adicionais, por favor contacte o nosso suporte.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't fail the rejection
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index), new { section = "validar-vendedores" });
        }

        // POST: Administrador/RejeitarVendedor/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarVendedor(int id)
        {
            var vendedor = await _db.Vendedores.FindAsync(id);
            if (vendedor == null) return NotFound();

            vendedor.Estado = "Rejeitado";

            // Registar ação
            var admin = await GetCurrentAdminAsync();
            if (admin != null)
            {
                var acao = new AcaoUser
                {
                    UtilizadorId = id,
                    Data = DateTime.UtcNow,
                    AdministradorId = admin.Id,
                    Motivo = "Rejeitar: Vendedor Rejeitado"
                };
                _db.Add(acao);
            }

            await _db.SaveChangesAsync();

            TempData["Warning"] = $"Vendedor '{vendedor.Nome}' foi rejeitado.";

            // Notificar vendedor por email
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(vendedor.Email))
                {
                    await _emailSender.SendAsync(
                        vendedor.Email,
                        "Pedido de Vendedor - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #dc3545;'>Pedido não aprovado</h2>
                            <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                            <p>Lamentamos informar que o seu pedido para se tornar vendedor na plataforma <strong>404 Ride</strong> não foi aprovado neste momento.</p>
                            <p>Se necessitar de esclarecimentos adicionais, por favor contacte o nosso suporte.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index), new { section = "validar-vendedores" });
        }



        // POST: Administrador/CriarUtilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CriarUtilizador(string nome, string email, string password, string tipo, string? nif, string? nivelAcesso)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(tipo))
            {
                TempData["UserWarning"] = "Todos os campos obrigatórios devem ser preenchidos.";
                return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
            }

            // Verificar permissões para criar Administrador
            if (tipo == "Administrador")
            {
                var checkingAdmin = await GetCurrentAdminAsync();
                if (checkingAdmin == null) return Forbid();

                if (checkingAdmin.NivelAcesso == "Nivel 2")
                {
                    TempData["UserWarning"] = "Administradores de Nível 2 não podem criar outros administradores.";
                    return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
                }

                // Nivel 1 só pode criar Nivel 2
                if (checkingAdmin.NivelAcesso == "Nivel 1" && nivelAcesso == "Nivel 1")
                {
                    TempData["UserWarning"] = "Administradores de Nível 1 só podem criar administradores de Nível 2.";
                    return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
                }
            }

            // Verificar se email já existe
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                TempData["UserWarning"] = "Já existe um utilizador com este email.";
                return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
            }

            // Criar IdentityUser
            var user = new ApplicationUser
            {
                UserName = email.Split('@')[0], // Username simples baseado no email
                Email = email,
                FullName = nome,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["UserWarning"] = $"Erro ao criar utilizador: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
            }

            // Adicionar Role
            await _userManager.AddToRoleAsync(user, tipo);

            // Criar Entidade de Domínio
            if (tipo == "Comprador")
            {
                var comprador = new Comprador
                {
                    IdentityUserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Nome = nome,
                    Estado = "Ativo",
                    Tipo = "Comprador",
                    PasswordHash = "IDENTITY" // Placeholder
                };
                _db.Compradores.Add(comprador);
            }
            else if (tipo == "Vendedor")
            {
                var vendedor = new Vendedor
                {
                    IdentityUserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Nome = nome,
                    Nif = nif,
                    Estado = "Ativo", // Vendedores criados por admin já nascem ativos
                    Tipo = "Vendedor",
                    PasswordHash = "IDENTITY"
                };
                _db.Vendedores.Add(vendedor);
            }
            else if (tipo == "Administrador")
            {
                var admin = new Administrador
                {
                    IdentityUserId = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Nome = nome,
                    Estado = "Ativo",
                    Tipo = "Administrador",
                    NivelAcesso = nivelAcesso ?? "Nivel 2", // Default to Nivel 2 if not specified
                    PasswordHash = "IDENTITY"
                };
                _db.Administradores.Add(admin);
            }

            await _db.SaveChangesAsync();

            // Registar ação
            var currentAdmin = await GetCurrentAdminAsync();
            if (currentAdmin != null)
            {
                // Obter ID do novo utilizador (da tabela de domínio)
                int novoUserId = 0;
                if (tipo == "Comprador") novoUserId = (await _db.Compradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id))?.Id ?? 0;
                else if (tipo == "Vendedor") novoUserId = (await _db.Vendedores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id))?.Id ?? 0;
                else if (tipo == "Administrador") novoUserId = (await _db.Administradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id))?.Id ?? 0;

                if (novoUserId > 0)
                {
                    var acao = new AcaoUser
                    {
                        UtilizadorId = novoUserId,
                        Data = DateTime.UtcNow,
                        AdministradorId = currentAdmin.Id,
                        Motivo = $"Criar: Novo {tipo} criado manualmente"
                    };
                    _db.Add(acao);
                    await _db.SaveChangesAsync();
                }
            }

            TempData["UserSuccess"] = $"Utilizador '{nome}' ({tipo}) criado com sucesso.";
            return RedirectToAction(nameof(Index), new { section = "criar-utilizador" });
        }

        // POST: Administrador/BloquearUtilizador/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BloquearUtilizador(int id, string motivo, DateTime? dataFim)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["UserWarning"] = "Utilizador não encontrado.";
                return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
            }

            // Verificar hierarquia para bloquear administradores
            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrador");
            if (isAdmin)
            {
                var currentAdmin = await GetCurrentAdminAsync();
                var targetAdmin = await _db.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);
                
                bool canBlock = false;
                if (currentAdmin?.NivelAcesso == "Nivel 0") canBlock = true; // Root can block anyone
                else if (currentAdmin?.NivelAcesso == "Nivel 1" && targetAdmin?.NivelAcesso == "Nivel 2") canBlock = true; // Level 1 can block Level 2

                if (!canBlock)
                {
                    TempData["UserWarning"] = "Não tem permissão para bloquear este administrador.";
                    return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
                }
            }

            // Definir data de fim do bloqueio (default: 100 anos se não especificado)
            var lockoutEnd = dataFim.HasValue 
                ? new DateTimeOffset(dataFim.Value) 
                : DateTimeOffset.UtcNow.AddYears(100);

            // Bloquear
            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
            await _userManager.SetLockoutEnabledAsync(user, true);

            // Registar ação
            // Precisamos encontrar o ID do Utilizador (tabela Utilizador) correspondente ao IdentityUser
            var utilizador = await _db.Compradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador 
                             ?? await _db.Vendedores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador;

            if (utilizador != null)
            {
                var admin = await GetCurrentAdminAsync();
                if (admin != null)
                {
                    var acao = new AcaoUser
                    {
                        UtilizadorId = utilizador.Id,
                        Data = DateTime.UtcNow,
                        AdministradorId = admin.Id,
                        Motivo = "Bloquear: " + motivo
                    };
                    _db.Add(acao);
                    await _db.SaveChangesAsync();
                }
            }

            TempData["UserSuccess"] = $"Utilizador '{user.FullName ?? user.UserName}' foi bloqueado com sucesso.";

            // Enviar email de notificação
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(user.Email))
                {
                    var duracaoTexto = dataFim.HasValue 
                        ? $"até {dataFim.Value:dd/MM/yyyy}" 
                        : "permanentemente";

                    await _emailSender.SendAsync(
                        user.Email,
                        "Conta Bloqueada - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #dc3545;'>Conta Bloqueada</h2>
                            <p>Olá <strong>{user.FullName ?? user.UserName}</strong>,</p>
                            <p>A sua conta na plataforma <strong>404 Ride</strong> foi <span style='color: #dc3545;'>bloqueada</span> {duracaoTexto}.</p>
                            <p><strong>Motivo:</strong> {motivo}</p>
                            <p>Se acredita que isto é um erro, por favor contacte o nosso suporte.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
        }

        // POST: Administrador/DesbloquearUtilizador/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesbloquearUtilizador(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["UserWarning"] = "Utilizador não encontrado.";
                return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
            }

            // Desbloquear
            await _userManager.SetLockoutEndDateAsync(user, null);

            // Registar ação
            var utilizador = await _db.Compradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador 
                             ?? await _db.Vendedores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador;

            if (utilizador != null)
            {
                var admin = await GetCurrentAdminAsync();
                if (admin != null)
                {
                    var acao = new AcaoUser
                    {
                        UtilizadorId = utilizador.Id,
                        Data = DateTime.UtcNow,
                        AdministradorId = admin.Id,
                        Motivo = "Desbloquear: Desbloqueio manual"
                    };
                    _db.Add(acao);
                    await _db.SaveChangesAsync();
                }
            }

            TempData["UserSuccess"] = $"Utilizador '{user.FullName ?? user.UserName}' foi desbloqueado com sucesso.";

            // Enviar email de notificação
            try
            {
                if (_emailSender != null && !string.IsNullOrEmpty(user.Email))
                {
                    await _emailSender.SendAsync(
                        user.Email,
                        "Conta Desbloqueada - 404 Ride",
                        $@"<html>
                        <body style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #28a745;'>Conta Desbloqueada</h2>
                            <p>Olá <strong>{user.FullName ?? user.UserName}</strong>,</p>
                            <p>A sua conta na plataforma <strong>404 Ride</strong> foi <span style='color: #28a745;'>desbloqueada</span>.</p>
                            <p>Já pode voltar a aceder à sua conta normalmente.</p>
                            <hr>
                            <p style='color: #666; font-size: 12px;'>Esta é uma mensagem automática. Por favor não responda a este email.</p>
                        </body>
                        </html>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar email: {ex.Message}");
            }

            return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
        }

        // POST: Administrador/EliminarUtilizador/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUtilizador(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                TempData["UserWarning"] = "Utilizador não encontrado.";
                return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
            }

            // Verificar hierarquia para eliminar administradores
            var isAdmin = await _userManager.IsInRoleAsync(user, "Administrador");
            if (isAdmin)
            {
                var currentAdmin = await GetCurrentAdminAsync();
                var targetAdmin = await _db.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);
                
                bool canDelete = false;
                if (currentAdmin?.NivelAcesso == "Nivel 0") canDelete = true; // Root can delete anyone
                else if (currentAdmin?.NivelAcesso == "Nivel 1" && targetAdmin?.NivelAcesso == "Nivel 2") canDelete = true; // Level 1 can delete Level 2

                if (!canDelete)
                {
                    TempData["UserWarning"] = "Não tem permissão para eliminar este administrador.";
                    return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
                }
            }

            var userName = user.FullName ?? user.UserName;

            // Eliminar registos relacionados na base de dados
            var isVendedor = await _userManager.IsInRoleAsync(user, "Vendedor");
            var isComprador = await _userManager.IsInRoleAsync(user, "Comprador");

            if (isVendedor)
            {
                var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.IdentityUserId == user.Id);
                if (vendedor != null)
                {
                    // Eliminar anúncios do vendedor
                    var anuncios = await _db.Anuncios.Where(a => a.VendedorId == vendedor.Id).ToListAsync();
                    _db.Anuncios.RemoveRange(anuncios);

                    _db.Vendedores.Remove(vendedor);
                }
            }

            if (isComprador)
            {
                var comprador = await _db.Compradores.FirstOrDefaultAsync(c => c.IdentityUserId == user.Id);
                if (comprador != null)
                {
                    _db.Compradores.Remove(comprador);
                }
            }

            // Eliminar o utilizador do Identity
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _db.SaveChangesAsync();
                TempData["UserSuccess"] = $"Utilizador '{userName}' foi eliminado com sucesso.";
            }
            else
            {
                TempData["UserWarning"] = $"Erro ao eliminar utilizador: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index), new { section = "gerir-utilizadores" });
        }

        // POST: Administrador/EliminarAnuncio/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarAnuncio(int id)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.Imagens)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null)
            {
                TempData["AnuncioWarning"] = "Anúncio não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var tituloAnuncio = anuncio.Titulo;

            // Eliminar imagens relacionadas
            if (anuncio.Imagens != null && anuncio.Imagens.Any())
            {
                _db.Imagens.RemoveRange(anuncio.Imagens);
            }

            // Eliminar o anúncio
            _db.Anuncios.Remove(anuncio);
            await _db.SaveChangesAsync();

            TempData["AnuncioSuccess"] = $"Anúncio '{tituloAnuncio}' foi eliminado com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Administrador/PausarAnuncio/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PausarAnuncio(int id)
        {
            var anuncio = await _db.Anuncios
                .Include(a => a.AcoesAnuncio)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (anuncio == null)
            {
                TempData["AnuncioWarning"] = "Anúncio não encontrado.";
                return RedirectToAction(nameof(Index), new { section = "moderar-anuncios" });
            }

            var user = await _userManager.GetUserAsync(User);
            var admin = await _db.Administradores.FirstOrDefaultAsync(a => a.IdentityUserId == user.Id);

            if (admin == null)
            {
                TempData["AnuncioWarning"] = "Erro ao identificar administrador.";
                return RedirectToAction(nameof(Index), new { section = "moderar-anuncios" });
            }

            // Verificar estado atual
            var lastAction = anuncio.AcoesAnuncio
                .OrderByDescending(a => a.Data)
                .FirstOrDefault();

            bool isPaused = lastAction?.Motivo == "Anúncio Pausado";
            string message = isPaused ? "retomado" : "pausado";

            // Criar nova ação
            var acao = new AcaoAnuncio
            {
                AnuncioId = id,
                Data = DateTime.UtcNow,
                // TipoAcao é o discriminador (AcaoAnuncio), não devemos definir manualmente
                AdministradorId = admin.Id,
                Motivo = isPaused ? "Anúncio Retomado" : "Anúncio Pausado"
            };

            _db.Add(acao);
            await _db.SaveChangesAsync();

            TempData["AnuncioSuccess"] = $"Anúncio '{anuncio.Titulo}' foi {message} com sucesso.";
            return RedirectToAction(nameof(Index), new { section = "moderar-anuncios" });
        }
    }
}

