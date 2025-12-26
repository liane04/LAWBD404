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

                var tipo = TryGetTipo(db, a.tipo ?? "Carro");
                if (tipo == null) { log($"[seed-skip] tipo desconhecido '{a.tipo}' no anuncio '{a.titulo}'"); continue; }

                var marca = EnsureMarca(db, a.marca);
                if (marca == null) { log($"[seed-skip] marca desconhecida '{a.marca}' no anuncio '{a.titulo}'"); continue; }

                var modelo = EnsureModelo(db, a.modelo, marca, tipo);
                if (modelo == null) { log($"[seed-skip] modelo desconhecido '{a.modelo}' no anuncio '{a.titulo}'"); continue; }

                var categoria = string.IsNullOrWhiteSpace(a.categoria) 
                    ? TryGetCategoria(db, "Geral") 
                    : TryGetCategoria(db, a.categoria);
                
                if (categoria == null) 
                {
                     // Tenta fallback para "Geral" se a especifica falhar
                     categoria = TryGetCategoria(db, "Geral");
                     if (categoria == null) { log($"[seed-skip] categoria desconhecida '{a.categoria}' no anuncio '{a.titulo}'"); continue; }
                }

                var combustivel = string.IsNullOrWhiteSpace(a.combustivel) ? null : TryGetCombustivel(db, a.combustivel);
                // Se combustivel for invalido mas fornecido, ignoramos combustivel ou saltamos? 
                // Assumindo que pode ser null, mas se fornecido e nao existe, é dados sujos. Vamos saltar.
                if (!string.IsNullOrWhiteSpace(a.combustivel) && combustivel == null)
                {
                    log($"[seed-skip] combustivel desconhecido '{a.combustivel}' no anuncio '{a.titulo}'");
                    continue;
                }

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

        private static Tipo? TryGetTipo(ApplicationDbContext db, string nome)
        {
            return db.Tipos.FirstOrDefault(t => t.Nome == nome);
        }

        private static Marca EnsureMarca(ApplicationDbContext db, string nome)
        {
            // Marcas e Modelos ainda são dinâmicos ou vêm do ReferenceDataSeeder, mas se o anúncio traz uma marca nova, vamos assumir que o sistema permite (ou poderiamos restringir também).
            // O user pediu explicitamente "categorias" e referiu "esses aspetos" (provavelmente tipos, combustiveis).
            // Manterei EnsureMarca para flexibilidade, ou mudo para TryGet dependendo do rigor.
            // Dado que o user disse "marca, modelo... tenham ficheiro proprio... ou pelo menos nao sejam feitos a patir do veiculos",
            // ReferenceDataSeeder JÁ carrega marcas do car-list.json. Então aqui deviamos apenas fazer lookup.
            return db.Marcas.FirstOrDefault(m => m.Nome == nome);
        }

        private static Modelo EnsureModelo(ApplicationDbContext db, string nome, Marca marca, Tipo tipo)
        {
            return db.Modelos.FirstOrDefault(m => m.Nome == nome && m.MarcaId == marca.Id);
        }

        private static Categoria? TryGetCategoria(ApplicationDbContext db, string nome)
        {
            return db.Categorias.FirstOrDefault(c => c.Nome == nome);
        }

        private static Combustivel? TryGetCombustivel(ApplicationDbContext db, string tipo)
        {
            return db.Combustiveis.FirstOrDefault(c => c.Tipo == tipo);
        }
    }
}
