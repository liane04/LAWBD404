using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class DenunciaAnuncio : Denuncia
    {
        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        public int? VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor? Vendedor { get; set; } // Vendedor que responde/gere a denúncia
    }
}
