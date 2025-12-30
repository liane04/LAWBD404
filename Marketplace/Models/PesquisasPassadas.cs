using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class PesquisasPassadas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        public int Count { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Utilizador Comprador { get; set; } = null!;

        [StringLength(500)]
        public string? Parametros { get; set; }

        [StringLength(200)]
        public string? Descricao { get; set; }

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
