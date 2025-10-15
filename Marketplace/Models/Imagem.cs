using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Imagem
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(500)]
        public string ImagemCaminho { get; set; } = null!;

        public int? AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio? Anuncio { get; set; }
    }
}
