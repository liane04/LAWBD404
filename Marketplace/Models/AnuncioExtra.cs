using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class AnuncioExtra
    {
        [Key]
        public int Id { get; set; }

        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        public int ExtraId { get; set; }
        [ForeignKey("ExtraId")]
        public Extra Extra { get; set; } = null!;
    }
}
