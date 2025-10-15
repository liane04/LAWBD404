using Marketplace.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    public abstract class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(60, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!;

        [Required, StringLength(120)]
        public string Nome { get; set; } = null!;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(30)]
        public string? Estado { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; }

        public int? MoradaId { get; set; }
        [ForeignKey("MoradaId")]
        public Morada? Morada { get; set; }

        public ICollection<AcaoUser> AcoesUser { get; set; } = new List<AcaoUser>();
        public ICollection<DenunciaUser> DenunciasRecebidas { get; set; } = new List<DenunciaUser>();

    }
}
