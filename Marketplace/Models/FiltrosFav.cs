using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class FiltrosFav
    {
        [Key]
        public int Id { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;

        // Metadados do filtro guardado
        [StringLength(100)]
        public string? Nome { get; set; }

        // Critérios de pesquisa guardados
        public int? MarcaId { get; set; }
        public int? ModeloId { get; set; }
        public int? TipoId { get; set; }
        public int? CombustivelId { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoMax { get; set; }

        public int? AnoMin { get; set; }
        public int? AnoMax { get; set; }
        public int? KmMax { get; set; }

        [StringLength(50)]
        public string? Caixa { get; set; }

        [StringLength(100)]
        public string? Localizacao { get; set; }

        // Alertas/Notificações
        public bool Ativo { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastCheckedAt { get; set; }

        // Heurística para identificar novidades (IDs crescentes de Anuncio)
        public int MaxAnuncioIdNotificado { get; set; } = 0;

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
