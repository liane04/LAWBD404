using Marketplace.Data;
using Marketplace.Services;
using Marketplace.Data.Seeders;
using Marketplace.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

// DbContext (Identity + domínio)
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

// External authentication providers (Google) - register only if configured
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    builder.Services
        .AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });
}

// Email sender (SMTP)
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
// Background service para notificações de filtros guardados
builder.Services.AddHostedService<SavedFiltersNotificationService>();

// Stripe Configuration
Stripe.StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

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

// Garantir charset UTF-8 nas respostas HTML (mitiga problemas de encoding)
app.Use(async (context, next) =>
{
    await next();
    var ct = context.Response.ContentType;
    if (!string.IsNullOrEmpty(ct) && ct.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
    {
        if (!ct.Contains("charset", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.ContentType = "text/html; charset=utf-8";
        }
    }
});

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

// Seed data only in Development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    var env = services.GetRequiredService<IHostEnvironment>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

    try
    {
        db.Database.Migrate();
        await ReferenceDataSeeder.SeedAsync(db, env.ContentRootPath, Console.WriteLine);
        await UserSeeder.SeedAsync(userManager, roleManager, db, env.ContentRootPath, Console.WriteLine);
        await AnuncioSeeder.SeedAsync(db, env.ContentRootPath, Console.WriteLine);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Seeding error: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
        }
    }
}

app.Run();
