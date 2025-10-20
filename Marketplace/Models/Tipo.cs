using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Tipo
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        // Relação 1:N - Um tipo (ex: Carro) pode ter vários modelos
        public ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
    }
}
