using System.Collections.Generic;
using Marketplace.Models;

namespace Marketplace.Models.ViewModels
{
    public class PerfilViewModel
    {
        // Identidade
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? ImagemPerfil { get; set; }

        // Morada
        public string? Rua { get; set; }
        public string? Localidade { get; set; }
        public string? CodigoPostal { get; set; }

        // Roles
        public bool IsVendedor { get; set; }
        public bool IsComprador { get; set; }
        public bool IsAdministrador { get; set; }

        // Vendedor
        public IEnumerable<Anuncio>? MeusAnuncios { get; set; }
        public int AnunciosCount { get; set; }
        public int ReservasRecebidasCount { get; set; }
        public int VisitasAgendadasCount { get; set; }
    }
}

