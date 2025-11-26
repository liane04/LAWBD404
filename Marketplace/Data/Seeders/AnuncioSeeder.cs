using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Marketplace.Models;

namespace Marketplace.Data.Seeders
{
    public static class AnuncioSeeder
    {
        private record AnuncioSeed(
            string titulo,
            string descricao,
            decimal preco,
            int? ano,
            int? quilometragem,
            string? cor,
            string? caixa,
            string? localizacao,
            int? portas,
            int? lugares,
            int? potencia,
            int? cilindrada,
            string marca,
            string modelo,
            string tipo,
            string? categoria,
            string? combustivel);

        public static async Task SeedAsync(ApplicationDbContext db, string contentRootPath, Action<string>? log = null)
        {
            log ??= _ => { };

            if (db.Anuncios.Any())
            {
                log("[seed] anúncios já existem, a ignorar");
                return;
            }

            var vendedores = db.Vendedores.ToList();
            if (vendedores.Count == 0)
            {
                log("[seed] sem vendedor disponível; correr UserSeeder primeiro");
                return;
            }

            var path = Path.Combine(contentRootPath, "Data", "Seeds", "anuncios-demo.json");
            if (!File.Exists(path))
            {
                log("[seed] anuncios-demo.json não encontrado, a ignorar anúncios demo");
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var doc = JsonSerializer.Deserialize<Dictionary<string, List<AnuncioSeed>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var seeds = doc != null && doc.TryGetValue("anuncios", out var list) ? list : new List<AnuncioSeed>();

            int vendedorIndex = 0;
            foreach (var a in seeds)
            {
                if (string.IsNullOrWhiteSpace(a.titulo) || a.preco <= 0 || string.IsNullOrWhiteSpace(a.marca) || string.IsNullOrWhiteSpace(a.modelo))
                    continue;

                var tipo = EnsureTipo(db, a.tipo ?? "Carro");
                var marca = EnsureMarca(db, a.marca);
                var modelo = EnsureModelo(db, a.modelo, marca, tipo);
                var categoria = string.IsNullOrWhiteSpace(a.categoria) ? EnsureCategoria(db, "Geral") : EnsureCategoria(db, a.categoria);
                var combustivel = string.IsNullOrWhiteSpace(a.combustivel) ? null : EnsureCombustivel(db, a.combustivel);

                var vendedor = vendedores[vendedorIndex % vendedores.Count];
                vendedorIndex++;

                // evitar duplicados por título e vendedor
                if (db.Anuncios.Any(x => x.Titulo == a.titulo && x.VendedorId == vendedor.Id))
                    continue;

                var anuncio = new Anuncio
                {
                    Titulo = a.titulo,
                    Descricao = a.descricao,
                    Preco = a.preco,
                    Ano = a.ano,
                    Quilometragem = a.quilometragem,
                    Cor = a.cor,
                    Caixa = a.caixa,
                    Localizacao = a.localizacao,
                    Portas = a.portas,
                    Lugares = a.lugares,
                    Potencia = a.potencia,
                    Cilindrada = a.cilindrada,
                    VendedorId = vendedor.Id,
                    MarcaId = marca.Id,
                    ModeloId = modelo.Id,
                    CategoriaId = categoria.Id,
                    CombustivelId = combustivel?.Id,
                    TipoId = tipo.Id,
                    NVisualizacoes = 0,
                    ValorSinal = 0
                };

                db.Anuncios.Add(anuncio);
            }

            await db.SaveChangesAsync();
            log($"[seed] inseridos {seeds.Count} anúncios demo (quando válidos)");
        }

        private static Tipo EnsureTipo(ApplicationDbContext db, string nome)
        {
            var tipo = db.Tipos.FirstOrDefault(t => t.Nome == nome);
            if (tipo != null) return tipo;
            tipo = new Tipo { Nome = nome };
            db.Tipos.Add(tipo);
            db.SaveChanges();
            return tipo;
        }

        private static Marca EnsureMarca(ApplicationDbContext db, string nome)
        {
            var marca = db.Marcas.FirstOrDefault(m => m.Nome == nome);
            if (marca != null) return marca;
            marca = new Marca { Nome = nome };
            db.Marcas.Add(marca);
            db.SaveChanges();
            return marca;
        }

        private static Modelo EnsureModelo(ApplicationDbContext db, string nome, Marca marca, Tipo tipo)
        {
            var modelo = db.Modelos.FirstOrDefault(m => m.Nome == nome && m.MarcaId == marca.Id);
            if (modelo != null) return modelo;
            modelo = new Modelo { Nome = nome, MarcaId = marca.Id, TipoId = tipo.Id };
            db.Modelos.Add(modelo);
            db.SaveChanges();
            return modelo;
        }

        private static Categoria EnsureCategoria(ApplicationDbContext db, string nome)
        {
            var cat = db.Categorias.FirstOrDefault(c => c.Nome == nome);
            if (cat != null) return cat;
            cat = new Categoria { Nome = nome };
            db.Categorias.Add(cat);
            db.SaveChanges();
            return cat;
        }

        private static Combustivel EnsureCombustivel(ApplicationDbContext db, string tipo)
        {
            var comb = db.Combustiveis.FirstOrDefault(c => c.Tipo == tipo);
            if (comb != null) return comb;
            comb = new Combustivel { Tipo = tipo };
            db.Combustiveis.Add(comb);
            db.SaveChanges();
            return comb;
        }
    }
}
