using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Combustivel
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = null!;

        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
    }
}
