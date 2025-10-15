using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Morada
    {
        [Key]
        [ForeignKey(nameof(Utilizador))]
        public int UtilizadorId { get; set; }

        [Required, StringLength(160)]
        public string Descricao { get; set; } = null!;

        [Required, StringLength(8)]
        [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "Formato 0000-000")]
        public string CodigoPostal { get; set; } = null!;

        [Required, StringLength(80)]
        public string Localidade { get; set; } = null!;

        public Utilizador Utilizador { get; set; } = null!;
    }
}
