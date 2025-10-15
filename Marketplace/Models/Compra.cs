using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Compra
    {
        [Key]
        public int Id { get; set; }

        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        [Required]
        public DateTime Data { get; set; }

        [StringLength(30)]
        public string? EstadoPagamento { get; set; }
    }
}
