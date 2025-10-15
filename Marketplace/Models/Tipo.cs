using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Tipo
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
    }
}
