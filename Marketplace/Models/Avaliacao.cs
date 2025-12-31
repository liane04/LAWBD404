using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Avaliacao
    {
        [Key]
        public int Id { get; set; }

        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        public DateTime Data { get; set; } = DateTime.Now;

        [Range(1, 5)]
        public int Nota { get; set; } // 1 a 5 estrelas

        [StringLength(500)]
        public string? Comentario { get; set; }
    }
}
