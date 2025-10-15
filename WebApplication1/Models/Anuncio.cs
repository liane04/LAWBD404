using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Anuncio
    {
        [Key]
        public int Id { get; set; }

        [Required, Column(TypeName = "decimal(10,2)")]
        public decimal Preco { get; set; }

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
    }
}
