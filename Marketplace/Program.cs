using Marketplace.Data;
using Marketplace.Services;
using Marketplace.Data.Seeders;
using Marketplace.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// DbContexts
builder.Services.AddDbContext<MarketplaceContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketplaceContext") ?? throw new InvalidOperationException("Connection string 'MarketplaceContext' not found.")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// MVC
builder.Services.AddControllersWithViews();

// ASP.NET Core Identity
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        // Email & User
        options.SignIn.RequireConfirmedEmail = false; // DESATIVADO temporariamente para testes
        options.User.RequireUniqueEmail = true;

        // Password policy - Segurança reforçada
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false; // Não obrigar caracteres especiais para facilitar
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 3;

        // Lockout policy - Proteção contra brute force
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Email sender (SMTP)
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Utilizadores/Login";
    options.AccessDeniedPath = "/Home/StatusCode/403";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.Name = "DriveDeal.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// Middleware para páginas de status code personalizadas (404, 403, 500, etc.)
app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Register email sender service (SMTP)
using (var smtpScope = app.Services.CreateScope())
{
    var services = smtpScope.ServiceProvider;
    var emailSender = services.GetService<Marketplace.Services.IEmailSender>();
    if (emailSender == null)
    {
        // Register default SMTP sender if not already registered elsewhere
        var sc = app.Services as IServiceProvider;
    }
}

// Seed default users (admin, seller, buyer) on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    try
    {
        db.Database.Migrate();
        // TEMPORARIAMENTE DESATIVADO - problemas com estrutura BD
        // await ReferenceDataSeeder.SeedAsync(db, env.ContentRootPath, s => System.Console.WriteLine(s));
        Console.WriteLine("⏭️  ReferenceDataSeeder desativado temporariamente");

        // Ensure roles - IMPORTANTE: Criar sempre no arranque
        string[] roles = new[] { "Administrador", "Vendedor", "Comprador" };
        foreach (var r in roles)
        {
            if (!await roleManager.RoleExistsAsync(r))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<int> { Name = r });
                if (result.Succeeded)
                    Console.WriteLine($"✅ Role '{r}' criado com sucesso");
                else
                    Console.WriteLine($"❌ Erro ao criar role '{r}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
            else
            {
                Console.WriteLine($"✓ Role '{r}' já existe");
            }
        }

        // Seed default users via Identity
        async Task<ApplicationUser> EnsureUserAsync(string email, string username, string fullName, string role, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };
                var createResult = await userManager.CreateAsync(user, password);
                if (!createResult.Succeeded)
                {
                    Console.WriteLine($"❌ Erro ao criar utilizador {email}: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                    return null;
                }

                var roleResult = await userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    Console.WriteLine($"❌ Erro ao adicionar role {role} ao utilizador {email}: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
                else
                {
                    Console.WriteLine($"✅ Utilizador {email} criado com role {role}");
                }
            }
            else
            {
                Console.WriteLine($"✓ Utilizador {email} já existe");
            }
            return user;
        }

        // Passwords fortes que cumprem os requisitos: 8+ caracteres, maiúsculas, minúsculas, dígitos
        var adminUser = await EnsureUserAsync("admin@email.com", "admin", "Administrador", "Administrador", "Admin123");
        var vendUser = await EnsureUserAsync("vendedor@email.com", "vendedor", "Vendedor Demo", "Vendedor", "Vende123");
        var compUser = await EnsureUserAsync("comprador@email.com", "comprador", "Comprador Demo", "Comprador", "Compr123");

        // Ensure domain entities linked to Identity users
        if (adminUser != null && !db.Administradores.Any(a => a.IdentityUserId == adminUser.Id))
        {
            db.Administradores.Add(new Marketplace.Models.Administrador
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

        if (vendUser != null && !db.Vendedores.Any(v => v.IdentityUserId == vendUser.Id))
        {
            db.Vendedores.Add(new Marketplace.Models.Vendedor
            {
                Username = vendUser.UserName!,
                Email = vendUser.Email!,
                Nome = vendUser.FullName ?? "Vendedor Demo",
                PasswordHash = "IDENTITY",
                Estado = "Ativo",
                Tipo = "Vendedor",
                IdentityUserId = vendUser.Id
            });
        }

        if (compUser != null && !db.Compradores.Any(c => c.IdentityUserId == compUser.Id))
        {
            db.Compradores.Add(new Marketplace.Models.Comprador
            {
                Username = compUser.UserName!,
                Email = compUser.Email!,
                Nome = compUser.FullName ?? "Comprador Demo",
                PasswordHash = "IDENTITY",
                Estado = "Ativo",
                Tipo = "Comprador",
                IdentityUserId = compUser.Id
            });
        }

        db.SaveChanges();
        Console.WriteLine("✅ Seeding de dados completo!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERRO no seeding: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
}

app.Run();
