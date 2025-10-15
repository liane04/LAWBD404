using Marketplace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Administrador : Utilizador
    {
        [StringLength(50)]
        public string? NivelAcesso { get; set; }

        public ICollection<HistoricoAcao> HistoricoAcoes { get; set; } = new List<HistoricoAcao>();
        public ICollection<Denuncia> DenunciasGeridas { get; set; } = new List<Denuncia>();
    }
}
