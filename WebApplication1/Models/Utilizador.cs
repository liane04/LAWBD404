using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public abstract class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(60, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = null!

        [Required, StringLength(120)]
        public string Nome { get; set; } = null!;

        [Required, StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(30)]
        public string? Estado { get; set; }  // "ativo", "bloqueado", ...

        
        public Morada? Morada { get; set; }
    }
}
