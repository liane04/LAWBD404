// Programa temporário para criar utilizadores padrão manualmente
// Execute: dotnet run --project Marketplace.csproj SeedDatabase.cs

using Marketplace.Data;
using Marketplace.Models;
using Marketplace.Services;
using Microsoft.EntityFrameworkCore;

public class SeedDatabase
{
    public static void Main(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=MarketplaceDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        using var db = new ApplicationDbContext(optionsBuilder.Options);

        // Verificar se já existem utilizadores
        if (db.Set<Utilizador>().Any())
        {
            Console.WriteLine("❌ Já existem utilizadores na base de dados!");
            return;
        }

        Console.WriteLine("🔧 A criar utilizadores padrão...");

        // Hash da password "123" para todos
        string hashed = PasswordHasher.HashPassword("123");

        // Criar Administrador
        var admin = new Administrador
        {
            Username = "admin",
            Email = "admin@email.com",
            Nome = "Administrador",
            PasswordHash = hashed,
            Estado = "Ativo",
            Tipo = "Administrador",
            NivelAcesso = "Total"
        };

        // Criar Vendedor
        var vendedor = new Vendedor
        {
            Username = "vendedor",
            Email = "vendedor@email.com",
            Nome = "Vendedor Demo",
            PasswordHash = hashed,
            Estado = "Ativo",
            Tipo = "Vendedor"
        };

        // Criar Comprador
        var comprador = new Comprador
        {
            Username = "comprador",
            Email = "comprador@email.com",
            Nome = "Comprador Demo",
            PasswordHash = hashed,
            Estado = "Ativo",
            Tipo = "Comprador"
        };

        // Adicionar à base de dados
        db.Administradores.Add(admin);
        db.Vendedores.Add(vendedor);
        db.Compradores.Add(comprador);
        db.SaveChanges();

        Console.WriteLine("✅ Utilizadores criados com sucesso!");
        Console.WriteLine();
        Console.WriteLine("📋 Contas criadas:");
        Console.WriteLine("   👨‍💼 Admin:     admin@email.com / 123");
        Console.WriteLine("   🏢 Vendedor:  vendedor@email.com / 123");
        Console.WriteLine("   🛒 Comprador: comprador@email.com / 123");
    }
}
