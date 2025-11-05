using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marketplace.Models;

namespace Marketplace.Data.Seeders
{
    public static class ReferenceDataSeeder
    {
        private record CarListItem(string brand, string[] models);

        public static async Task SeedAsync(ApplicationDbContext db, string contentRootPath, Action<string>? log = null)
        {
            log ??= _ => { };

            // Ensure base Tipo "Carro" exists (Id = 1 as configured in HasData).
            var tipoCarro = await db.Tipos.FindAsync(1);
            if (tipoCarro == null)
            {
                tipoCarro = new Tipo { Id = 1, Nome = "Carro" };
                db.Tipos.Add(tipoCarro);
                await db.SaveChangesAsync();
            }

            var path = Path.Combine(contentRootPath, "Data", "Seeds", "car-list.json");
            if (!File.Exists(path))
            {
                log($"Seed: ficheiro não encontrado: {path}");
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var items = JsonSerializer.Deserialize<List<CarListItem>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<CarListItem>();

            // Cache existing marcas and modelos
            var marcasByName = db.Marcas.ToDictionary(m => m.Nome, StringComparer.OrdinalIgnoreCase);

            foreach (var item in items)
            {
                if (string.IsNullOrWhiteSpace(item.brand)) continue;

                if (!marcasByName.TryGetValue(item.brand, out var marca))
                {
                    marca = new Marca { Nome = item.brand.Trim() };
                    db.Marcas.Add(marca);
                    await db.SaveChangesAsync();
                    marcasByName[marca.Nome] = marca;
                    log($"+ Marca: {marca.Nome}");
                }

                // Load existing modelos for this brand
                var existingModelNames = db.Modelos
                    .Where(m => m.MarcaId == marca.Id)
                    .Select(m => m.Nome)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var modelName in item.models ?? Array.Empty<string>())
                {
                    if (string.IsNullOrWhiteSpace(modelName)) continue;
                    if (existingModelNames.Contains(modelName)) continue;

                    db.Modelos.Add(new Modelo
                    {
                        Nome = modelName.Trim(),
                        MarcaId = marca.Id,
                        TipoId = tipoCarro.Id
                    });
                    log($"  + Modelo: {marca.Nome} {modelName}");
                }
                await db.SaveChangesAsync();
            }
        }
    }
}

