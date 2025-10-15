using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Vendedor : Utilizador
    {
        [StringLength(9)]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O NIF deve ter 9 dígitos.")]
        public string? Nif { get; set; }

        // "particular" ou "empresa" (por agora string; depois podemos trocar para enum)
        [Required, StringLength(20)]
        public string Tipo { get; set; } = "particular";

        // Texto livre para dados de faturação (morada de faturação, empresa, etc.)
        [StringLength(255)]
        public string? DadosFaturacao { get; set; }

        public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
    }
}
