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
                log("[seed] anÇ§ncios jÇ­ existem, a ignorar");
                return;
            }

            var vendedores = db.Vendedores.ToList();
            if (vendedores.Count == 0)
            {
                log("[seed] sem vendedor disponÇðvel; correr UserSeeder primeiro");
                return;
            }

            var path = Path.Combine(contentRootPath, "Data", "Seeds", "anuncios-demo.json");
            if (!File.Exists(path))
            {
                log("[seed] anuncios-demo.json nÇœo encontrado, a ignorar anÇ§ncios demo");
                return;
            }

            var json = await File.ReadAllTextAsync(path);
            var doc = JsonSerializer.Deserialize<Dictionary<string, List<AnuncioSeed>>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var seeds = doc != null && doc.TryGetValue("anuncios", out var list) ? list : new List<AnuncioSeed>();

            int vendedorIndex = 0;
            var anunciosCriados = new List<Anuncio>();
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

                // evitar duplicados por tÇðtulo e vendedor
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
                anunciosCriados.Add(anuncio);
            }

            await db.SaveChangesAsync();
            await SeedImagensAsync(db, contentRootPath, anunciosCriados, log);
            log($"[seed] inseridos {anunciosCriados.Count} anÇ§ncios demo (quando vÇ­lidos)");
        }

        private static async Task SeedImagensAsync(ApplicationDbContext db, string contentRootPath, List<Anuncio> anuncios, Action<string> log)
        {
            if (anuncios.Count == 0)
                return;

            var imagensOrigem = GetSeedImagePool(contentRootPath);
            if (imagensOrigem.Count == 0)
            {
                log("[seed] anÇ§ncios inseridos mas sem imagens demo (nenhuma imagem de origem encontrada em Data/Seeds/images/anuncios ou wwwroot/imagens)");
                return;
            }

            var destinoRoot = Path.Combine(contentRootPath, "wwwroot", "images", "anuncios");
            Directory.CreateDirectory(destinoRoot);

            int offset = 0;
            foreach (var anuncio in anuncios)
            {
                var destinoAnuncio = Path.Combine(destinoRoot, anuncio.Id.ToString());
                Directory.CreateDirectory(destinoAnuncio);

                // Selecionar 3 imagens rotativas (ou menos se a pool for pequena)
                var selecionadas = Enumerable.Range(0, Math.Min(3, imagensOrigem.Count))
                    .Select(i => imagensOrigem[(offset + i) % imagensOrigem.Count])
                    .ToList();

                int idx = 1;
                foreach (var origem in selecionadas)
                {
                    var ext = Path.GetExtension(origem);
                    var nome = $"foto-{idx:00}{ext}";
                    var destino = Path.Combine(destinoAnuncio, nome);

                    File.Copy(origem, destino, overwrite: true);

                    db.Imagens.Add(new Imagem
                    {
                        AnuncioId = anuncio.Id,
                        ImagemCaminho = $"/images/anuncios/{anuncio.Id}/{nome}"
                    });

                    idx++;
                }

                offset++;
            }

            await db.SaveChangesAsync();
            log($"[seed] adicionadas imagens demo a {anuncios.Count} anÇ§ncios");
        }

        private static List<string> GetSeedImagePool(string contentRootPath)
        {
            var extensoesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };

            // 1) Tentar wwwroot/images/seeds/anuncios (pasta dedicada para imagens de seed)
            var seedsDir = Path.Combine(contentRootPath, "wwwroot", "images", "seeds", "anuncios");
            if (Directory.Exists(seedsDir))
            {
                var files = Directory.GetFiles(seedsDir).Where(f => extensoesPermitidas.Contains(Path.GetExtension(f))).ToList();
                if (files.Count > 0) return files;
            }

            // 2) Fallback: usar imagens existentes em wwwroot/imagens
            var wwwrootImagens = Path.Combine(contentRootPath, "wwwroot", "imagens");
            if (Directory.Exists(wwwrootImagens))
            {
                var files = Directory.GetFiles(wwwrootImagens).Where(f => extensoesPermitidas.Contains(Path.GetExtension(f))).ToList();
                if (files.Count > 0) return files;
            }

            return new List<string>();
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
