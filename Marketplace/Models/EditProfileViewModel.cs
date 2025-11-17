using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class EditProfileViewModel
    {
        [Required, StringLength(120, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        // Morada
        [StringLength(200)]
        public string? MoradaRua { get; set; }

        [StringLength(20)]
        public string? MoradaCodigoPostal { get; set; }

        [StringLength(100)]
        public string? MoradaLocalidade { get; set; }

        // Upload da imagem de perfil
        public IFormFile? ProfileImage { get; set; }

        public bool RemoverImagem { get; set; } = false;

        // Apenas informativos
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? ImagemAtual { get; set; }
    }
}

