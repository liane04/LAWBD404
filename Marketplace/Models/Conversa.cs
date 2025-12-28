using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Conversa
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = null!; // "A comprar" ou "A anunciar"

        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        public DateTime UltimaMensagemData { get; set; }

        public ICollection<Mensagens> Mensagens { get; set; } = new List<Mensagens>();
    }
}
