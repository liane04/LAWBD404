using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Modelo
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nome { get; set; } = null!;

        // FK para Marca
        public int MarcaId { get; set; }
        [ForeignKey(nameof(MarcaId))]
        public Marca Marca { get; set; } = null!;

        // FK para Tipo
        public int TipoId { get; set; }
        [ForeignKey(nameof(TipoId))]
        public Tipo Tipo { get; set; } = null!;

        // Um modelo pode estar em vários anúncios
        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
    }
}
