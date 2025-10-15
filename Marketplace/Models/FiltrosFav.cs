using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class FiltrosFav
    {
        [Key]
        public int Id { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
