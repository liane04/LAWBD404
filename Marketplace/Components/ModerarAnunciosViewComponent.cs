using System.Linq;
using System.Threading.Tasks;
using Marketplace.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Marketplace.Models;

namespace Marketplace.Components
{
    public class ModerarAnunciosViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;

        public ModerarAnunciosViewComponent(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Obter todos os anúncios com informações relacionadas
            var anuncios = await _db.Anuncios
                .Include(a => a.Vendedor)
                .Include(a => a.Marca)
                .Include(a => a.Modelo)
                .Include(a => a.Tipo)
                .Include(a => a.Categoria)
                .Include(a => a.Combustivel)
                .Include(a => a.Imagens)
                .Include(a => a.Denuncias)
                .Include(a => a.AcoesAnuncio)
                .OrderByDescending(a => a.Id)
                .ToListAsync();

            // Criar lista de anúncios detalhados
            var anunciosDetalhados = anuncios.Select(a => new AnuncioDetalhadoVM
            {
                Id = a.Id,
                Titulo = a.Titulo,
                Preco = a.Preco,
                Ano = a.Ano,
                Quilometragem = a.Quilometragem,
                Localizacao = a.Localizacao,
                NVisualizacoes = a.NVisualizacoes,
                VendedorNome = a.Vendedor?.Nome ?? "N/A",
                VendedorId = a.VendedorId,
                MarcaNome = a.Marca?.Nome ?? "N/A",
                ModeloNome = a.Modelo?.Nome ?? "N/A",
                TipoNome = a.Tipo?.Nome ?? "N/A",
                CategoriaNome = a.Categoria?.Nome ?? "N/A",
                CombustivelTipo = a.Combustivel?.Tipo ?? "N/A",
                PrimeiraImagem = a.Imagens?.FirstOrDefault()?.ImagemCaminho,
                HasDenuncias = a.Denuncias != null && a.Denuncias.Any(),
                IsPausado = a.AcoesAnuncio != null && a.AcoesAnuncio.OrderByDescending(ac => ac.Data).FirstOrDefault()?.Motivo == "Anúncio Pausado"
            }).ToList();

            // Calcular estatísticas
            var model = new ModerarAnunciosSectionVM
            {
                Anuncios = anunciosDetalhados,
                TotalPendentes = 0, // Pode ser implementado se adicionar campo Estado
                TotalAtivos = anunciosDetalhados.Count(a => !a.HasDenuncias && !a.IsPausado),
                TotalDenuncias = anunciosDetalhados.Count(a => a.HasDenuncias),
                TotalPausados = anunciosDetalhados.Count(a => a.IsPausado)
            };

            return View(model);
        }
    }

    public class ModerarAnunciosSectionVM
    {
        public System.Collections.Generic.List<AnuncioDetalhadoVM> Anuncios { get; set; } = new();
        public int TotalPendentes { get; set; }
        public int TotalAtivos { get; set; }
        public int TotalDenuncias { get; set; }
        public int TotalPausados { get; set; }
    }

    public class AnuncioDetalhadoVM
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public int? Ano { get; set; }
        public int? Quilometragem { get; set; }
        public string? Localizacao { get; set; }
        public int NVisualizacoes { get; set; }
        public string VendedorNome { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        public string MarcaNome { get; set; } = string.Empty;
        public string ModeloNome { get; set; } = string.Empty;
        public string TipoNome { get; set; } = string.Empty;
        public string CategoriaNome { get; set; } = string.Empty;
        public string CombustivelTipo { get; set; } = string.Empty;
        public string? PrimeiraImagem { get; set; }
        public bool HasDenuncias { get; set; }
        public bool IsPausado { get; set; }
    }
}
