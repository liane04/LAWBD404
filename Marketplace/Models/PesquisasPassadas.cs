using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class PesquisasPassadas
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? QueryString { get; set; } // Stores the full query string e.g. "?marcaId=1&precoMax=5000"

        [StringLength(200)]
        public string? Descricao { get; set; } // Human readable description e.g. "BMW, < 5000€"

        public int UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public Utilizador Utilizador { get; set; } = null!;

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
