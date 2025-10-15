using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class DenunciaUser : Denuncia
    {
        public int UtilizadorAlvoId { get; set; }
        [ForeignKey("UtilizadorAlvoId")]
        public Utilizador UtilizadorAlvo { get; set; } = null!;
    }
}
