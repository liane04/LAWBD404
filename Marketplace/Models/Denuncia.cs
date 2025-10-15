using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public abstract class Denuncia
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(2000)]
        public string Descricao { get; set; } = null!;

        [StringLength(30)]
        public string? Estado { get; set; } // "pendente", "em análise", "resolvida", "rejeitada"

        [Required]
        public DateTime DataDeDenuncia { get; set; }

        public DateTime? DataEncerramento { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        public int? AdministradorId { get; set; }
        [ForeignKey("AdministradorId")]
        public Administrador? Administrador { get; set; }
    }
}
