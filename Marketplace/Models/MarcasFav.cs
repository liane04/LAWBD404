using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class MarcasFav
    {
        [Key]
        public int Id { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Utilizador Comprador { get; set; } = null!;

        public int MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public Marca Marca { get; set; } = null!;

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
