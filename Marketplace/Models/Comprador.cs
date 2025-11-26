using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Comprador : Utilizador
    {
        [StringLength(500)]
        public string? Preferencias { get; set; }

        public ICollection<ContactosComprador> Contactos { get; set; } = new List<ContactosComprador>();
        public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
        public ICollection<Compra> Compras { get; set; } = new List<Compra>();
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
        public ICollection<PesquisasPassadas> PesquisasPassadas { get; set; } = new List<PesquisasPassadas>();
        public ICollection<Notificacoes> Notificacoes { get; set; } = new List<Notificacoes>();
        public ICollection<FiltrosFav> FiltrosFavoritos { get; set; } = new List<FiltrosFav>();
        public ICollection<AnuncioFav> AnunciosFavoritos { get; set; } = new List<AnuncioFav>();
        public ICollection<MarcasFav> MarcasFavoritas { get; set; } = new List<MarcasFav>();

        public ICollection<Conversa> Conversas { get; set; } = new List<Conversa>();
        public ICollection<Denuncia> Denuncias { get; set; } = new List<Denuncia>();
    }
}
