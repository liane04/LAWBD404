using Marketplace.Data;
using Marketplace.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

// Cookie Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
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

// Seed default users (admin, seller, buyer) on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        db.Database.Migrate();

        bool hasAnyUser = db.Set<Marketplace.Models.Utilizador>().Any();
        if (!hasAnyUser)
        {
            string hashed = PasswordHasher.HashPassword("123");

            var admin = new Marketplace.Models.Administrador
            {
                Username = "admin",
                Email = "admin@email.com",
                Nome = "Administrador",
                PasswordHash = hashed,
                Estado = "Ativo",
                Tipo = "Administrador",
                NivelAcesso = "Total"
            };

            var vendedor = new Marketplace.Models.Vendedor
            {
                Username = "vendedor",
                Email = "vendedor@email.com",
                Nome = "Vendedor Demo",
                PasswordHash = hashed,
                Estado = "Ativo",
                Tipo = "Vendedor"
            };

            var comprador = new Marketplace.Models.Comprador
            {
                Username = "comprador",
                Email = "comprador@email.com",
                Nome = "Comprador Demo",
                PasswordHash = hashed,
                Estado = "Ativo",
                Tipo = "Comprador"
            };

            db.Administradores.Add(admin);
            db.Vendedores.Add(vendedor);
            db.Compradores.Add(comprador);
            db.SaveChanges();
        }
    }
    catch
    {
        // ignore startup seeding errors in development
    }
}

app.Run();

