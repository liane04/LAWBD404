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


            // 1. Seed Tipos (File: types.json)
            await SeedReferenceEntity<Tipo>(db, contentRootPath, "types.json",
                (name) => new Tipo { Nome = name },
                (name) => db.Tipos.Any(x => x.Nome == name),
                log, "Tipos");

            // 2. Seed Categorias (File: categories.json)
            await SeedReferenceEntity<Categoria>(db, contentRootPath, "categories.json",
                (name) => new Categoria { Nome = name },
                (name) => db.Categorias.Any(x => x.Nome == name),
                log, "Categorias");

            // 3. Seed Combustiveis (File: fuels.json)
            await SeedReferenceEntity<Combustivel>(db, contentRootPath, "fuels.json",
                (name) => new Combustivel { Tipo = name },
                (name) => db.Combustiveis.Any(x => x.Tipo == name),
                log, "Combustiveis");

            // 4. Seed Marcas e Modelos (File: car-list.json)
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

            // Cache existing marcas
            var marcasByName = db.Marcas.ToDictionary(m => m.Nome, StringComparer.OrdinalIgnoreCase);

            // Get Default Type ID for "Carro" (assuming most in car-list are cars)
            // If "Carro" doesn't exist yet from step 1, we create it just in case, though step 1 should have covered it.
            var tipoCarroId = db.Tipos.FirstOrDefault(t => t.Nome == "Carro")?.Id
                              ?? db.Tipos.FirstOrDefault()?.Id; // Fallback

            if (tipoCarroId == null)
            {
                // Safety net if types.json was empty or failed
                var t = new Tipo { Nome = "Carro" };
                db.Tipos.Add(t);
                await db.SaveChangesAsync();
                tipoCarroId = t.Id;
            }

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
                        TipoId = tipoCarroId.Value
                    });
                    // log($"  + Modelo: {marca.Nome} {modelName}");
                }
                await db.SaveChangesAsync();
            }
        }

        private static async Task SeedReferenceEntity<T>(ApplicationDbContext db, string rootPath, string fileName, Func<string, T> factory, Func<string, bool> exists, Action<string> log, string entityName) where T : class
        {
            var path = Path.Combine(rootPath, "Data", "Seeds", fileName);
            if (!File.Exists(path))
            {
                log($"⚠️ Seed {entityName}: ficheiro não encontrado ({fileName})");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(path);
                var names = JsonSerializer.Deserialize<List<string>>(json);

                if (names == null) return;

                int count = 0;
                foreach (var name in names)
                {
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    if (!exists(name))
                    {
                        db.Set<T>().Add(factory(name));
                        count++;
                    }
                }

                if (count > 0)
                {
                    await db.SaveChangesAsync();
                    log($"✅ {entityName}: {count} novos registos adicionados.");
                }
            }
            catch (Exception ex)
            {
                log($"❌ Erro ao seedar {entityName}: {ex.Message}");
            }

        }
    }
}

