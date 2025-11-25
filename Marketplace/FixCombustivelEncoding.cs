using Microsoft.EntityFrameworkCore;
using Marketplace.Data;
using Marketplace.Models;

namespace Marketplace;

public class FixCombustivelEncoding
{
    public static async Task ExecuteAsync(ApplicationDbContext context)
    {
        Console.WriteLine("Corrigindo encoding dos tipos de combustível...");

        var combustiveis = await context.Combustiveis.ToListAsync();

        foreach (var combustivel in combustiveis)
        {
            var tipoOriginal = combustivel.Tipo;

            // Corrigir tipos com encoding errado
            if (combustivel.Tipo.Contains("l") && combustivel.Tipo.Contains("trico"))
            {
                combustivel.Tipo = "Elétrico";
            }
            else if (combustivel.Tipo.Contains("brido"))
            {
                combustivel.Tipo = "Híbrido";
            }
            else if (combustivel.Tipo.Contains("drog") && combustivel.Tipo.Contains("nio"))
            {
                combustivel.Tipo = "Hidrogénio";
            }

            if (tipoOriginal != combustivel.Tipo)
            {
                Console.WriteLine($"ID {combustivel.Id}: '{tipoOriginal}' -> '{combustivel.Tipo}'");
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Correção concluída!");

        // Listar resultados
        Console.WriteLine("\nTipos de combustível atualizados:");
        foreach (var c in combustiveis.OrderBy(c => c.Id))
        {
            Console.WriteLine($"  {c.Id}: {c.Tipo}");
        }
    }
}
