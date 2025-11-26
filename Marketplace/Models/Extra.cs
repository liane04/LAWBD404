using System.ComponentModel.DataAnnotations;

namespace Marketplace.Models
{
    public class Extra
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Descricao { get; set; } = null!;

        [Required, StringLength(50)]
        public string Tipo { get; set; } = null!; // Segurança, Conforto, Multimédia, Outros

        // Relacionamento N:N com Anuncio através de AnuncioExtra
        public ICollection<AnuncioExtra> AnuncioExtras { get; set; } = new List<AnuncioExtra>();
    }
}
