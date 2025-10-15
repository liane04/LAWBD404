using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Notificacoes
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(500)]
        public string Conteudo { get; set; } = null!;

        [Required]
        public DateTime Data { get; set; }

        public int? PesquisasPassadasId { get; set; }
        [ForeignKey("PesquisasPassadasId")]
        public PesquisasPassadas? PesquisasPassadas { get; set; }

        public int? FiltrosFavId { get; set; }
        [ForeignKey("FiltrosFavId")]
        public FiltrosFav? FiltrosFav { get; set; }

        public int? AnuncioFavId { get; set; }
        [ForeignKey("AnuncioFavId")]
        public AnuncioFav? AnuncioFav { get; set; }

        public int? MarcasFavId { get; set; }
        [ForeignKey("MarcasFavId")]
        public MarcasFav? MarcasFav { get; set; }

        public int CompradorId { get; set; }
        [ForeignKey("CompradorId")]
        public Comprador Comprador { get; set; } = null!;
    }
}
