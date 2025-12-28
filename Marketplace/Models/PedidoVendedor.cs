using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class PedidoVendedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompradorId { get; set; }

        [ForeignKey("CompradorId")]
        public Comprador? Comprador { get; set; }

        [Required]
        [StringLength(9, MinimumLength = 9)]
        public string Nif { get; set; } = string.Empty;

        [StringLength(500)]
        public string? DadosFaturacao { get; set; }

        [Required]
        [StringLength(1000)]
        public string Motivacao { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendente"; // Pendente, Aprovado, Rejeitado

        [Required]
        public DateTime DataPedido { get; set; } = DateTime.Now;

        public DateTime? DataResposta { get; set; }

        public int? AdminRespondeuId { get; set; }

        [StringLength(500)]
        public string? MotivoRejeicao { get; set; }
    }
}
