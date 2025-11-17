using System.Collections.Generic;

namespace Marketplace.Models
{
    public class ProfileViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Estado { get; set; } = "Ativo";

        public string? ImagemPerfilUrl { get; set; }

        // Morada
        public string? MoradaRua { get; set; }
        public string? MoradaCodigoPostal { get; set; }
        public string? MoradaLocalidade { get; set; }
        public string? MoradaPais { get; set; } = "Portugal";

        // Roles atuais (opcional para futuras melhorias na view)
        public IEnumerable<string> Roles { get; set; } = new List<string>();

        // Pedido para Vendedor
        public bool SellerRequestPending { get; set; }
    }
}
