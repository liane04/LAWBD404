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
                AnunciosSuspensos = (await _db.Anuncios
                                    .Select(a => new { 
                                        a.Id, 
                                        LastAction = a.AcoesAnuncio.OrderByDescending(ac => ac.Data).Select(ac => ac.Motivo).FirstOrDefault() 
                                    })
                                    .ToListAsync())
                                    .Count(x => x.LastAction == "Anúncio Pausado"),
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
            public int AnunciosSuspensos { get; set; }
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAdminLevel(int id, string nivel)
        {
            var currentUser = await GetCurrentAdminAsync();
            if (currentUser == null || currentUser.NivelAcesso != "Nivel 0")
            {
                return Forbid();
            }

            var adminToUpdate = await _db.Administradores.FindAsync(id);
            if (adminToUpdate == null)
            {
                return NotFound();
            }

            // Prevent changing own level to lock oneself out (or allow it if intended, but let's warn)
            if (adminToUpdate.Id == currentUser.Id && nivel != "Nivel 0")
            {
                TempData["UserWarning"] = "Não pode alterar o seu próprio nível de acesso para um nível inferior.";
                return RedirectToAction(nameof(Index), new { section = "gerir-admins" });
            }

            var oldLevel = adminToUpdate.NivelAcesso;
            adminToUpdate.NivelAcesso = nivel;
            
            // Registar ação no histórico
            var acao = new AcaoUser
            {
                UtilizadorId = adminToUpdate.Id,
                Data = DateTime.UtcNow,
                AdministradorId = currentUser.Id,
                Motivo = $"Alteração de Nível: {oldLevel} -> {nivel}",
                TipoAcao = "AcaoUser" // Explicitly setting discriminator if needed, though EF handles it usually. Keeping consistent with previous fixes.
            };
            _db.AcoesUser.Add(acao);
            
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nível de acesso atualizado com sucesso.";
            return RedirectToAction(nameof(Index), new { section = "gerir-admins" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverPermissoesAdmin(int id)
        {
            var currentUser = await GetCurrentAdminAsync();
            if (currentUser == null || currentUser.NivelAcesso != "Nivel 0")
            {
                return Forbid();
            }

            // Using NoTracking or just finding entity.
            // Note: If we change discriminator via SQL, EF Core context tracking might get confused if we reused 'adminToUpdate'.
            // But here we just need ID and IdentityUserId.
            var adminToUpdate = await _db.Administradores.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            
            if (adminToUpdate == null) return NotFound();

            if (adminToUpdate.Id == currentUser.Id)
            {
                TempData["ErrorMessage"] = "Não pode remover as suas próprias permissões.";
                return RedirectToAction(nameof(Index), new { section = "gerir-admins" });
            }

            // 1. Update Identity Roles
            var identityUser = await _userManager.FindByIdAsync(adminToUpdate.IdentityUserId.ToString());
            if (identityUser != null)
            {
                if (await _userManager.IsInRoleAsync(identityUser, "Administrador"))
                {
                    await _userManager.RemoveFromRoleAsync(identityUser, "Administrador");
                }
                if (!await _userManager.IsInRoleAsync(identityUser, "Comprador"))
                {
                    await _userManager.AddToRoleAsync(identityUser, "Comprador");
                }
            }

            // 2. Update Domain Entity (Raw SQL to change Discriminator)
            // Assumes table name is "Utilizador" (verified in snapshot).
            // Also clearing NivelAcesso to NULL as Compradores don't use it.
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE Utilizador SET Discriminator = 'Comprador', NivelAcesso = NULL WHERE Id = {0}", 
                id
            );

            // 3. Log History
            var acao = new AcaoUser
            {
                UtilizadorId = id, // ID remains valid
                Data = DateTime.UtcNow,
                AdministradorId = currentUser.Id,
                Motivo = "Despromoção: Permissões de administrador removidas",
                TipoAcao = "AcaoUser"
            };
            _db.AcoesUser.Add(acao);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Administrador removido com sucesso. O utilizador é agora um Comprador.";
            return RedirectToAction(nameof(Index), new { section = "gerir-admins" });
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
            // Bloquear
            user.LockoutEnd = lockoutEnd;
            user.LockoutEnabled = true; // Forçar ativação do lockout
            await _userManager.UpdateAsync(user);

            // Registar ação
            // Precisamos encontrar o ID do Utilizador (tabela Utilizador) correspondente ao IdentityUser
            var utilizador = await _db.Compradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador 
                             ?? await _db.Vendedores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador;

            if (utilizador != null)
            {
                var admin = await GetCurrentAdminAsync();
                if (admin != null)
                {
                    string motivoCompleto = "Bloquear: " + motivo;
                    if (!await IsDuplicateUserAction(utilizador.Id, admin.Id, motivoCompleto))
                    {
                        var acao = new AcaoUser
                        {
                            UtilizadorId = utilizador.Id,
                            Data = DateTime.UtcNow,
                            AdministradorId = admin.Id,
                            Motivo = motivoCompleto
                        };
                        _db.Add(acao);
                        await _db.SaveChangesAsync();
                    }
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
            // Desbloquear
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            // Registar ação
            var utilizador = await _db.Compradores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador 
                             ?? await _db.Vendedores.FirstOrDefaultAsync(u => u.IdentityUserId == user.Id) as Utilizador;

            if (utilizador != null)
            {
                var admin = await GetCurrentAdminAsync();
                if (admin != null)
                {
                    string motivo = "Desbloquear: Desbloqueio manual";
                    if (!await IsDuplicateUserAction(utilizador.Id, admin.Id, motivo))
                    {
                        var acao = new AcaoUser
                        {
                            UtilizadorId = utilizador.Id,
                            Data = DateTime.UtcNow,
                            AdministradorId = admin.Id,
                            Motivo = motivo
                        };
                        _db.Add(acao);
                        await _db.SaveChangesAsync();
                    }
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
            return RedirectToAction(nameof(Index), new { section = "moderar-anuncios" });
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
            string motivoAcao = isPaused ? "Anúncio Retomado" : "Anúncio Pausado";
            
            if (!await IsDuplicateAnuncioAction(id, admin.Id, motivoAcao))
            {
                var acao = new AcaoAnuncio
                {
                    AnuncioId = id,
                    Data = DateTime.UtcNow,
                    AdministradorId = admin.Id,
                    Motivo = motivoAcao
                };

                _db.Add(acao);
                await _db.SaveChangesAsync();
            }

            TempData["AnuncioSuccess"] = $"Anúncio '{anuncio.Titulo}' foi {message} com sucesso.";
            return RedirectToAction(nameof(Index), new { section = "moderar-anuncios" });
        }
        // ====================================================================================
        // HELPERS PARA DEDUPLICAÇÃO DE AÇÕES
        // ====================================================================================

        private async Task<bool> IsDuplicateUserAction(int userId, int adminId, string motivo)
        {
            var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30);
            return await _db.AcoesUser.AnyAsync(a =>
                a.UtilizadorId == userId &&
                a.AdministradorId == adminId &&
                a.Motivo == motivo &&
                a.Data >= thirtySecondsAgo);
        }

        private async Task<bool> IsDuplicateAnuncioAction(int anuncioId, int adminId, string motivo)
        {
            var thirtySecondsAgo = DateTime.UtcNow.AddSeconds(-30);
            return await _db.AcoesAnuncio.AnyAsync(a =>
                a.AnuncioId == anuncioId &&
                a.AdministradorId == adminId &&
                a.Motivo == motivo &&
                a.Data >= thirtySecondsAgo);
        }


        // POST: Administrador/AprovarPedidoVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarPedidoVendedor(int id)
        {
            var pedido = await _db.PedidosVendedor
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado != "Pendente")
            {
                TempData["Error"] = "Este pedido já foi processado.";
                return RedirectToAction(nameof(Index));
            }

            var currentAdmin = await GetCurrentAdminAsync();
            if (currentAdmin == null)
            {
                TempData["Error"] = "Administrador não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var comprador = pedido.Comprador;
                if (comprador == null)
                {
                    TempData["Error"] = "Comprador não encontrado.";
                    return RedirectToAction(nameof(Index));
                }

                // Adicionar role de Vendedor ao utilizador (sem criar novo registo na tabela)
                var appUser = await _userManager.FindByIdAsync(comprador.IdentityUserId.ToString());
                if (appUser != null)
                {
                    var isVendedor = await _userManager.IsInRoleAsync(appUser, "Vendedor");
                    if (!isVendedor)
                    {
                        await _userManager.AddToRoleAsync(appUser, "Vendedor");
                    }
                }

                // Atualizar pedido
                pedido.Estado = "Aprovado";
                pedido.DataResposta = DateTime.Now;
                pedido.AdminRespondeuId = currentAdmin.Id;

                await _db.SaveChangesAsync();

                // Enviar email de confirmação
                if (_emailSender != null && !string.IsNullOrWhiteSpace(comprador.Email))
                {
                    await EnviarEmailAprovacao(comprador, pedido);
                }

                TempData["Success"] = $"Pedido de {comprador.Nome} aprovado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao aprovar pedido: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Administrador/RejeitarPedidoVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarPedidoVendedor(int id, string? motivoRejeicao)
        {
            var pedido = await _db.PedidosVendedor
                .Include(p => p.Comprador)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pedido == null)
            {
                TempData["Error"] = "Pedido não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            if (pedido.Estado != "Pendente")
            {
                TempData["Error"] = "Este pedido já foi processado.";
                return RedirectToAction(nameof(Index));
            }

            var currentAdmin = await GetCurrentAdminAsync();
            if (currentAdmin == null)
            {
                TempData["Error"] = "Administrador não encontrado.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                pedido.Estado = "Rejeitado";
                pedido.DataResposta = DateTime.Now;
                pedido.AdminRespondeuId = currentAdmin.Id;
                pedido.MotivoRejeicao = motivoRejeicao;

                await _db.SaveChangesAsync();

                // Enviar email de rejeição
                if (_emailSender != null && pedido.Comprador != null && !string.IsNullOrWhiteSpace(pedido.Comprador.Email))
                {
                    await EnviarEmailRejeicao(pedido.Comprador, pedido);
                }

                TempData["Success"] = $"Pedido de {pedido.Comprador?.Nome} rejeitado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao rejeitar pedido: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task EnviarEmailAprovacao(Comprador comprador, PedidoVendedor pedido)
        {
            if (_emailSender == null || string.IsNullOrWhiteSpace(comprador.Email)) return;

            var subject = "✅ Pedido Aprovado - Bem-vindo como Vendedor 404 Ride";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>404 Ride</h1>
                        <p style='color: white; margin: 10px 0 0 0;'>Marketplace de Veículos Usados</p>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px;'>
                        <h2 style='color: #333; margin-top: 0;'>Parabéns, {comprador.Nome}!</h2>
                        <p>O seu pedido para tornar-se vendedor foi <strong style='color: #10b981;'>aprovado</strong>!</p>

                        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #10b981;'>
                            <h3 style='margin-top: 0; color: #10b981;'>O que pode fazer agora:</h3>
                            <ul style='line-height: 1.8;'>
                                <li>Criar e gerir os seus anúncios de veículos</li>
                                <li>Receber e responder a reservas</li>
                                <li>Agendar visitas com potenciais compradores</li>
                                <li>Gerir a sua disponibilidade</li>
                                <li>Acompanhar as suas vendas</li>
                            </ul>
                        </div>

                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='{Url.Action("Perfil", "Utilizadores", null, Request.Scheme)}'
                               style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Aceder ao Meu Perfil
                            </a>
                        </p>

                        <p style='text-align: center; margin: 20px 0;'>
                            <a href='{Url.Action("Create", "Anuncios", null, Request.Scheme)}'
                               style='background: #10b981; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Criar Primeiro Anúncio
                            </a>
                        </p>

                        <div style='background: #e0e7ff; padding: 15px; border-radius: 5px; margin-top: 20px;'>
                            <p style='margin: 0; color: #3730a3;'>
                                <strong>💡 Dica:</strong> Certifique-se de preencher todos os dados de faturação no seu perfil para receber os pagamentos das vendas.
                            </p>
                        </div>

                        <p style='color: #666; font-size: 12px; text-align: center; margin-top: 30px;'>
                            Este é um email automático do sistema 404 Ride.<br>
                            Bem-vindo à nossa comunidade de vendedores!
                        </p>
                    </div>
                </div>";

            await _emailSender.SendAsync(comprador.Email, subject, message);
        }

        private async Task EnviarEmailRejeicao(Comprador comprador, PedidoVendedor pedido)
        {
            if (_emailSender == null || string.IsNullOrWhiteSpace(comprador.Email)) return;

            var subject = "❌ Pedido Não Aprovado - 404 Ride";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>404 Ride</h1>
                        <p style='color: white; margin: 10px 0 0 0;'>Marketplace de Veículos Usados</p>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px;'>
                        <h2 style='color: #333; margin-top: 0;'>Olá, {comprador.Nome}</h2>
                        <p>Infelizmente, o seu pedido para tornar-se vendedor não foi aprovado neste momento.</p>

                        {(!string.IsNullOrWhiteSpace(pedido.MotivoRejeicao) ? $@"
                        <div style='background: white; padding: 20px; border-radius: 8px; margin: 20px 0; border-left: 4px solid #ef4444;'>
                            <h4 style='margin-top: 0; color: #ef4444;'>Motivo:</h4>
                            <p>{pedido.MotivoRejeicao}</p>
                        </div>" : "")}

                        <p>Pode submeter um novo pedido após corrigir as questões mencionadas.</p>

                        <p style='color: #666; font-size: 12px; text-align: center; margin-top: 30px;'>
                            Este é um email automático do sistema 404 Ride.<br>
                            Para mais informações, contacte-nos.
                        </p>
                    </div>
                </div>";

            await _emailSender.SendAsync(comprador.Email, subject, message);
        }

        // POST: Administrador/AprovarVendedor (versão simplificada)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarVendedor(int id)
        {
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.Id == id);

            if (vendedor == null)
            {
                TempData["Warning"] = "Vendedor não encontrado.";
                return RedirectToAction("Index");
            }

            if (vendedor.Estado != "Pendente")
            {
                TempData["Warning"] = "Este vendedor já foi processado.";
                return RedirectToAction("Index");
            }

            try
            {
                // Atualizar estado do vendedor
                vendedor.Estado = "Ativo";

                // Adicionar role de Vendedor ao utilizador
                var appUser = await _userManager.FindByIdAsync(vendedor.IdentityUserId.ToString());
                if (appUser != null)
                {
                    var isInRole = await _userManager.IsInRoleAsync(appUser, "Vendedor");
                    if (!isInRole)
                    {
                        await _userManager.AddToRoleAsync(appUser, "Vendedor");
                    }
                }

                await _db.SaveChangesAsync();

                // Enviar email de confirmação
                if (_emailSender != null && !string.IsNullOrWhiteSpace(vendedor.Email))
                {
                    await EnviarEmailAprovacaoVendedor(vendedor);
                }

                TempData["Success"] = $"Vendedor {vendedor.Nome} aprovado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao aprovar vendedor: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        // POST: Administrador/RejeitarVendedor (versão simplificada)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarVendedor(int id, string? motivoRejeicao)
        {
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.Id == id);

            if (vendedor == null)
            {
                TempData["Warning"] = "Vendedor não encontrado.";
                return RedirectToAction("Index");
            }

            if (vendedor.Estado != "Pendente")
            {
                TempData["Warning"] = "Este vendedor já foi processado.";
                return RedirectToAction("Index");
            }

            try
            {
                vendedor.Estado = "Rejeitado";
                // Não temos campo Observacoes, então guardaremos no DadosFaturacao
                if (!string.IsNullOrWhiteSpace(motivoRejeicao))
                {
                    vendedor.DadosFaturacao = $"[REJEITADO] {motivoRejeicao}";
                }

                await _db.SaveChangesAsync();

                // Enviar email de rejeição
                if (_emailSender != null && !string.IsNullOrWhiteSpace(vendedor.Email))
                {
                    await EnviarEmailRejeicaoVendedor(vendedor, motivoRejeicao);
                }

                TempData["Success"] = $"Pedido de {vendedor.Nome} rejeitado.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao rejeitar pedido: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        private async Task EnviarEmailAprovacaoVendedor(Vendedor vendedor)
        {
            var subject = "✅ Pedido de Vendedor Aprovado - 404 Ride";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>404 Ride</h1>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px;'>
                        <h2 style='color: #28a745; margin-top: 0;'>Parabéns! Pedido Aprovado</h2>
                        <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                        <p>O seu pedido para se tornar vendedor na plataforma 404 Ride foi <strong>aprovado</strong>!</p>
                        <p>Agora pode começar a publicar os seus anúncios de veículos.</p>
                        <p style='text-align: center; margin: 30px 0;'>
                            <a href='{Url.Action("Perfil", "Utilizadores", null, Request.Scheme)}'
                               style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>
                                Aceder ao Perfil
                            </a>
                        </p>
                    </div>
                </div>";

            await _emailSender.SendAsync(vendedor.Email, subject, message);
        }

        private async Task EnviarEmailRejeicaoVendedor(Vendedor vendedor, string? motivo)
        {
            var subject = "❌ Pedido de Vendedor Rejeitado - 404 Ride";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <div style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding: 30px; text-align: center;'>
                        <h1 style='color: white; margin: 0;'>404 Ride</h1>
                    </div>
                    <div style='background: #f7f7f7; padding: 30px;'>
                        <h2 style='color: #dc3545; margin-top: 0;'>Pedido Rejeitado</h2>
                        <p>Olá <strong>{vendedor.Nome}</strong>,</p>
                        <p>Informamos que o seu pedido para se tornar vendedor foi <strong>rejeitado</strong>.</p>
                        {(!string.IsNullOrWhiteSpace(motivo) ? $@"
                        <div style='background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0;'>
                            <strong>Motivo:</strong> {motivo}
                        </div>" : "")}
                        <p>Pode submeter um novo pedido a qualquer momento.</p>
                    </div>
                </div>";

            await _emailSender.SendAsync(vendedor.Email, subject, message);
        }
    }
}

