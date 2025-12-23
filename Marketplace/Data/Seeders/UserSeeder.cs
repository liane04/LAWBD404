using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marketplace.Models;
using Microsoft.AspNetCore.Identity;

namespace Marketplace.Data.Seeders
{
    public static class UserSeeder
    {
        private record UserSeed(string fullName, string email, string username, string role, string? password);

        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ApplicationDbContext db,
            string contentRootPath,
            Action<string>? log = null)
        {
            log ??= _ => { };

            // Roles
            var roles = new[] { "Administrador", "Vendedor", "Comprador" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                    if (result.Succeeded)
                        log($"[seed] role '{role}' criada");
                    else
                        log($"[seed] falha a criar role '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            async Task<ApplicationUser?> EnsureUserAsync(string email, string username, string fullName, string role, string password)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user != null)
                {
                    log($"[seed] utilizador '{email}' jÇ­ existe");
                    return user;
                }

                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };

                var createRes = await userManager.CreateAsync(user, password);
                if (!createRes.Succeeded)
                {
                    log($"[seed] erro a criar utilizador '{email}': {string.Join(", ", createRes.Errors.Select(e => e.Description))}");
                    return null;
                }

                var roleRes = await userManager.AddToRoleAsync(user, role);
                if (!roleRes.Succeeded)
                {
                    log($"[seed] erro a adicionar role '{role}' ao utilizador '{email}': {string.Join(", ", roleRes.Errors.Select(e => e.Description))}");
                }
                else
                {
                    log($"[seed] utilizador '{email}' criado com role '{role}'");
                }

                return user;
            }

            // Utilizadores base mÇðnimos
            var adminUser = await EnsureUserAsync("admin@email.com", "admin", "Administrador", "Administrador", "Admin123");
            var vendedorUser = await EnsureUserAsync("vendedor@email.com", "vendedor", "Vendedor Demo", "Vendedor", "Vende123");
            var compradorUser = await EnsureUserAsync("comprador@email.com", "comprador", "Comprador Demo", "Comprador", "Compr123");

            // Entidades de domÇðnio associadas aos mÇðnimos
            if (adminUser != null && !db.Administradores.Any(a => a.IdentityUserId == adminUser.Id))
            {
                db.Administradores.Add(new Administrador
                {
                    Username = adminUser.UserName!,
                    Email = adminUser.Email!,
                    Nome = adminUser.FullName ?? "Administrador",
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = "Administrador",
                    NivelAcesso = "Total",
                    IdentityUserId = adminUser.Id
                });
            }

            if (vendedorUser != null && !db.Vendedores.Any(v => v.IdentityUserId == vendedorUser.Id))
            {
                db.Vendedores.Add(new Vendedor
                {
                    Username = vendedorUser.UserName!,
                    Email = vendedorUser.Email!,
                    Nome = vendedorUser.FullName ?? "Vendedor Demo",
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = "Vendedor",
                    IdentityUserId = vendedorUser.Id
                });
            }

            if (compradorUser != null && !db.Compradores.Any(c => c.IdentityUserId == compradorUser.Id))
            {
                db.Compradores.Add(new Comprador
                {
                    Username = compradorUser.UserName!,
                    Email = compradorUser.Email!,
                    Nome = compradorUser.FullName ?? "Comprador Demo",
                    PasswordHash = "IDENTITY",
                    Estado = "Ativo",
                    Tipo = "Comprador",
                    IdentityUserId = compradorUser.Id
                });
            }

            await db.SaveChangesAsync();

            // Mock users a partir do JSON
            var usersPath = Path.Combine(contentRootPath, "Data", "Seeds", "mock-users.json");
            List<UserSeed> seeds = new();
            if (!File.Exists(usersPath))
            {
                log("[seed] mock-users.json nÇœo encontrado, a ignorar utilizadores extra");
            }
            else
            {
                var json = await File.ReadAllTextAsync(usersPath);
                var doc = JsonSerializer.Deserialize<Dictionary<string, List<UserSeed>>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                seeds = doc != null && doc.TryGetValue("users", out var list) ? list : new List<UserSeed>();

                foreach (var u in seeds)
                {
                    if (string.IsNullOrWhiteSpace(u.email) || string.IsNullOrWhiteSpace(u.role) || string.IsNullOrWhiteSpace(u.username))
                        continue;

                    var pwd = string.IsNullOrWhiteSpace(u.password) ? "Demo123!" : u.password!;
                    var role = roles.Contains(u.role, StringComparer.OrdinalIgnoreCase)
                        ? roles.First(r => r.Equals(u.role, StringComparison.OrdinalIgnoreCase))
                        : "Comprador";

                    var created = await EnsureUserAsync(u.email, u.username, u.fullName ?? u.username, role, pwd);

                    if (created == null)
                        continue;

                    if (role == "Administrador" && !db.Administradores.Any(a => a.IdentityUserId == created.Id))
                    {
                        db.Administradores.Add(new Administrador
                        {
                            Username = created.UserName!,
                            Email = created.Email!,
                            Nome = created.FullName ?? created.UserName!,
                            PasswordHash = "IDENTITY",
                            Estado = "Ativo",
                            Tipo = "Administrador",
                            NivelAcesso = "Total",
                            IdentityUserId = created.Id
                        });
                    }
                    else if (role == "Vendedor" && !db.Vendedores.Any(v => v.IdentityUserId == created.Id))
                    {
                        db.Vendedores.Add(new Vendedor
                        {
                            Username = created.UserName!,
                            Email = created.Email!,
                            Nome = created.FullName ?? created.UserName!,
                            PasswordHash = "IDENTITY",
                            Estado = "Ativo",
                            Tipo = "Vendedor",
                            IdentityUserId = created.Id
                        });
                    }
                    else if (role == "Comprador" && !db.Compradores.Any(c => c.IdentityUserId == created.Id))
                    {
                        db.Compradores.Add(new Comprador
                        {
                            Username = created.UserName!,
                            Email = created.Email!,
                            Nome = created.FullName ?? created.UserName!,
                            PasswordHash = "IDENTITY",
                            Estado = "Ativo",
                            Tipo = "Comprador",
                            IdentityUserId = created.Id
                        });
                    }
                }
            }

            await db.SaveChangesAsync();

            await SeedProfileImagesAsync(db, contentRootPath, log);
        }

        private static async Task SeedProfileImagesAsync(ApplicationDbContext db, string contentRootPath, Action<string> log)
        {
            var pool = GetProfileImagePool(contentRootPath);
            if (pool.Count == 0)
            {
                log("[seed] utilizadores criados sem fotos demo (nenhuma imagem de origem encontrada em Data/Seeds/images/perfil ou wwwroot/imagens)");
                return;
            }

            var destinoPerfil = Path.Combine(contentRootPath, "wwwroot", "images", "perfil");
            Directory.CreateDirectory(destinoPerfil);

            // Mapear utilizadores de domÇðnio por IdentityUserId para atualizar o mesmo caminho de imagem
            var admins = db.Administradores.ToDictionary(a => a.IdentityUserId);
            var vendedores = db.Vendedores.ToDictionary(v => v.IdentityUserId);
            var compradores = db.Compradores.ToDictionary(c => c.IdentityUserId);

            int idx = 0;
            var usersSemFoto = db.Users.Where(u => string.IsNullOrWhiteSpace(u.ImagemPerfil)).ToList();
            foreach (var user in usersSemFoto)
            {
                var origem = pool[idx % pool.Count];
                var ext = Path.GetExtension(origem);
                var fileName = $"user_{user.Id}{ext}";
                var destino = Path.Combine(destinoPerfil, fileName);

                File.Copy(origem, destino, overwrite: true);

                var relativo = $"/images/perfil/{fileName}";
                user.ImagemPerfil = relativo;

                if (admins.TryGetValue(user.Id, out var admin))
                    admin.ImagemPerfil = relativo;
                if (vendedores.TryGetValue(user.Id, out var vendedor))
                    vendedor.ImagemPerfil = relativo;
                if (compradores.TryGetValue(user.Id, out var comprador))
                    comprador.ImagemPerfil = relativo;

                idx++;
            }

            await db.SaveChangesAsync();

            // Garantir avatar por omissÇœo usado pelo helper
            var defaultAvatar = Path.Combine(contentRootPath, "wwwroot", "images", "default-avatar.png");
            if (!File.Exists(defaultAvatar))
            {
                try
                {
                    File.Copy(pool.First(), defaultAvatar, overwrite: true);
                }
                catch
                {
                    // ignore se falhar
                }
            }
        }

        private static List<string> GetProfileImagePool(string contentRootPath)
        {
            var extensoesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

            // 1) Pasta opcional Data/Seeds/images/perfil (se o utilizador quiser colocar fotos de rosto)
            var seedsDir = Path.Combine(contentRootPath, "Data", "Seeds", "images", "perfil");
            if (Directory.Exists(seedsDir))
            {
                var files = Directory.GetFiles(seedsDir).Where(f => extensoesPermitidas.Contains(Path.GetExtension(f))).ToList();
                if (files.Count > 0) return files;
            }

            // 2) Fallback: usar imagens existentes em wwwroot/imagens (jÇ­ incluÇðdas)
            var wwwrootImagens = Path.Combine(contentRootPath, "wwwroot", "imagens");
            if (Directory.Exists(wwwrootImagens))
            {
                var files = Directory.GetFiles(wwwrootImagens).Where(f => extensoesPermitidas.Contains(Path.GetExtension(f))).ToList();
                if (files.Count > 0) return files;
            }

            return new List<string>();
        }

    }
}
