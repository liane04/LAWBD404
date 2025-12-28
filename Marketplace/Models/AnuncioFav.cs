using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class AnuncioFav
    {
        [Key]
        public int Id { get; set; }

        // Mapeado para a mesma coluna na BD (CompradorId) mas agora referenciando a tabela base Utilizadores
        // para permitir que Vendedores também tenham favoritos.
        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Utilizador Comprador { get; set; } = null!;

        public int AnuncioId { get; set; }
        [ForeignKey("AnuncioId")]
        public Anuncio Anuncio { get; set; } = null!;

        [StringLength(100)]
        public string? Campo { get; set; }

        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
    }
}
