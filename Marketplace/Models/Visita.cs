using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Visita
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [StringLength(30)]
        public string? Estado { get; set; }

        public int? ReservaId { get; set; }
        [ForeignKey("ReservaId")]
        public Reserva? Reserva { get; set; }

        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;
    }
}
