using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Morada
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string CodigoPostal { get; set; } = null!;

        [Required, StringLength(100)]
        public string Localidade { get; set; } = null!;

        [Required, StringLength(200)]
        public string Rua { get; set; } = null!;

        public ICollection<Utilizador> Utilizadores { get; set; } = new List<Utilizador>();
    }
}
