using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Visita
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A data da visita é obrigatória")]
        [Display(Name = "Data da Visita")]
        public DateTime Data { get; set; }

        [StringLength(30)]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendente"; // Pendente, Confirmada, Cancelada, Concluída

        [StringLength(500)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        // Relações obrigatórias
        [Required]
        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        [Required]
        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        [Required]
        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        // Relação opcional com reserva (caso a visita seja agendada após reserva)
        public int? ReservaId { get; set; }
        [ForeignKey("ReservaId")]
        public Reserva? Reserva { get; set; }

        // Auditoria
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [Display(Name = "Data de Atualização")]
        public DateTime? DataAtualizacao { get; set; }
    }
}
