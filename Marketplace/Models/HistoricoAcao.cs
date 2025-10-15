using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public abstract class HistoricoAcao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [StringLength(500)]
        public string? Motivo { get; set; }

        [Required, StringLength(100)]
        public string TipoAcao { get; set; } = null!;

        public int AdministradorId { get; set; }
        [ForeignKey("AdministradorId")]
        public Administrador Administrador { get; set; } = null!;
    }
}
