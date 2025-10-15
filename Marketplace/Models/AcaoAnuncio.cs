using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class AcaoAnuncio : HistoricoAcao
    {
        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;
    }
}
