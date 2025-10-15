using System;

namespace WebApplication1.Models
{
    public class Modelo
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        public int? MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public Marca? Marca { get; set; }

        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
    }
}
