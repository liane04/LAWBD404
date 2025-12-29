using Marketplace.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Vendedor : Utilizador
    {
        [StringLength(200)]
        public string? DadosFaturacao { get; set; }

        [StringLength(20)]
        public string? Nif { get; set; }

        [StringLength(21)]
        [Display(Name = "NIB")]
        public string? Nib { get; set; }

        public ICollection<Contactos> Contactos { get; set; } = new List<Contactos>();
        public ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<Conversa> Conversas { get; set; } = new List<Conversa>();
        public ICollection<DenunciaAnuncio> DenunciasRespondidas { get; set; } = new List<DenunciaAnuncio>();
    }
}
