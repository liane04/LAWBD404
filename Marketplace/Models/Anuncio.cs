using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Anuncio
    {
        [Key]
        public int Id { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

        [Range(1900, 2025, ErrorMessage = "O ano deve estar entre 1900 e 2025")]
        public int? Ano { get; set; }

        [StringLength(50)]
        public string? Cor { get; set; }

        [StringLength(2000)]
        public string? Descricao { get; set; }

        public int? Quilometragem { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = null!;

        [StringLength(50)]
        public string? Caixa { get; set; }

        [StringLength(100)]
        public string? Localizacao { get; set; }

        public int? Portas { get; set; }

        public int? Lugares { get; set; }

        public int? Potencia { get; set; } // em cv

        public int? Cilindrada { get; set; } // em cm³

        // Valor do sinal exigido para reserva
        [Column("Valor_sinal", TypeName = "decimal(10,2)")]
        public decimal ValorSinal { get; set; } = 0m;

        // Número de visualizações do anúncio
        [Column("n_visualizacoes")]
        public int NVisualizacoes { get; set; } = 0;

        // Relações com outras entidades
        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        public int? MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public Marca? Marca { get; set; }

        public int? ModeloId { get; set; }
        [ForeignKey("ModeloId")]
        public Modelo? Modelo { get; set; }

        public int? CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        public int? CombustivelId { get; set; }
        [ForeignKey("CombustivelId")]
        public Combustivel? Combustivel { get; set; }

        public int? TipoId { get; set; }
        [ForeignKey("TipoId")]
        public Tipo? Tipo { get; set; }

        public ICollection<Imagem> Imagens { get; set; } = new List<Imagem>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        public ICollection<Conversa> Conversas { get; set; } = new List<Conversa>();
        public ICollection<DenunciaAnuncio> Denuncias { get; set; } = new List<DenunciaAnuncio>();
        public ICollection<AcaoAnuncio> AcoesAnuncio { get; set; } = new List<AcaoAnuncio>();
        public ICollection<AnuncioExtra> AnuncioExtras { get; set; } = new List<AnuncioExtra>();
    }
}
