using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Marca
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        // Uma marca (ex: Audi) tem vários modelos
        public ICollection<Modelo> Modelos { get; set; } = new List<Modelo>();
    }
}
