using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public class Mensagens
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(2000)]
        public string Conteudo { get; set; } = null!;

        [StringLength(30)]
        public string? Estado { get; set; } // "enviado", "lido", "não lido", etc.

        [Required]
        public DateTime DataEnvio { get; set; }

        public int ConversaId { get; set; }
        [ForeignKey("ConversaId")]
        public Conversa Conversa { get; set; } = null!;
    }
}
