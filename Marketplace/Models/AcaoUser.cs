using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class AcaoUser : HistoricoAcao
    {
        public int UtilizadorId { get; set; }
        [ForeignKey("UtilizadorId")]
        public Utilizador Utilizador { get; set; } = null!;
    }
}
