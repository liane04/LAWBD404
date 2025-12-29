using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models.ViewModels
{
    public class EditarPerfilViewModel
    {
        [Required, StringLength(120)]
        public string Nome { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        [StringLength(30)]
        [RegularExpression(@"^(\+351\s?)?([29]\d{2}\s?\d{3}\s?\d{3})$", ErrorMessage = "Telefone deve ser PT válido (ex: 912 345 678 ou +351 912 345 678)")]
        public string? Telefone { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^[1235689]\d{8}$", ErrorMessage = "NIF inválido (9 dígitos e prefixo válido)")]
        public string? Nif { get; set; }

        [StringLength(21)]
        [RegularExpression(@"^\d{21}$", ErrorMessage = "NIB deve ter 21 dígitos")]
        [Display(Name = "NIB")]
        public string? Nib { get; set; }

        [StringLength(200)]
        public string? DadosFaturacao { get; set; }

        // Morada
        [StringLength(200)]
        public string? Rua { get; set; }

        [StringLength(100)]
        public string? Localidade { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "Código Postal deve ser 0000-000")]
        public string? CodigoPostal { get; set; }

        public string? ImagemPerfilAtual { get; set; }

        public bool IsVendedor { get; set; }
    }
}
