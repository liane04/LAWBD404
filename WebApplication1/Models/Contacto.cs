using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Nome { get; set; } = null!;

        // FK opcional para Comprador
        public int? CompradorId { get; set; }
        public Comprador? Comprador { get; set; }

        // FK opcional para Vendedor
        public int? VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}
