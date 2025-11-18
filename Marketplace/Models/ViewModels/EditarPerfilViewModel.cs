using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models.ViewModels
{
    public class EditarPerfilViewModel
    {
        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Nif { get; set; }

        [StringLength(200)]
        public string? DadosFaturacao { get; set; }

        public string? ImagemPerfilAtual { get; set; }

        public bool IsVendedor { get; set; }
    }
}

