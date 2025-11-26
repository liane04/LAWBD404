// Console runner opcional para popular a base com dados de desenvolvimento.
// Executar: dotnet run --project Marketplace.csproj --no-build -- SeedDatabase

using System;
using Marketplace.Data;
using Marketplace.Data.Seeders;
using Marketplace.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class SeedDatabase
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContext<ApplicationDbContext>(opt =>
                    opt.UseSqlServer(ctx.Configuration.GetConnectionString("DefaultConnection")));

                services.AddIdentity<ApplicationUser, IdentityRole<int>>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            });

        var host = builder.Build();

        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var env = services.GetRequiredService<IHostEnvironment>();

        db.Database.Migrate();

        await ReferenceDataSeeder.SeedAsync(db, env.ContentRootPath, Console.WriteLine);
        await UserSeeder.SeedAsync(userManager, roleManager, db, env.ContentRootPath, Console.WriteLine);
        await AnuncioSeeder.SeedAsync(db, env.ContentRootPath, Console.WriteLine);
    }
}
