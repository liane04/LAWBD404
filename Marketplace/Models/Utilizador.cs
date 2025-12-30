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

        // Legacy password field from pre-Identity auth. When using ASP.NET Identity,
        // this can be set to a placeholder (e.g., "IDENTITY").
        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = "IDENTITY";

        [StringLength(30)]
        public string? Estado { get; set; }

        [StringLength(50)]
        public string? Tipo { get; set; }

        [StringLength(500)]
        public string? ImagemPerfil { get; set; }

        public int? MoradaId { get; set; }
        [ForeignKey("MoradaId")]
        public Morada? Morada { get; set; }

        // Link to ASP.NET Core Identity user
        public int IdentityUserId { get; set; }

        [ForeignKey("IdentityUserId")]
        public ApplicationUser IdentityUser { get; set; } = null!;

        public ICollection<AcaoUser> AcoesUser { get; set; } = new List<AcaoUser>();
        public ICollection<DenunciaUser> DenunciasRecebidas { get; set; } = new List<DenunciaUser>();
        public ICollection<AnuncioFav> AnunciosFavoritos { get; set; } = new List<AnuncioFav>();
        public ICollection<MarcasFav> MarcasFavoritas { get; set; } = new List<MarcasFav>();
        public ICollection<PesquisasPassadas> PesquisasPassadas { get; set; } = new List<PesquisasPassadas>();

    }
}
