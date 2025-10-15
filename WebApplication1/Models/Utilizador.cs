using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public abstract class Utilizador
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O username é obrigatório")]
        [StringLength(60, MinimumLength = 3)]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        [StringLength(254)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(120)]
        public string Nome { get; set; } = null!;

        [Required(ErrorMessage = "A password é obrigatória")]
        [StringLength(255)]
        public string PasswordHash { get; set; } = null!;

        [StringLength(30)]
        public string? Estado { get; set; }  // ex.: "ativo", "bloqueado"
    }


