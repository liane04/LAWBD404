using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Comprador : Utilizador
    {
        [StringLength(200)]
        public string? Preferencias { get; set; }

        public ICollection<Contacto> Contactos { get; set; } = new List<Contacto>();
    }
}
