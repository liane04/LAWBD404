using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class Administrador : Utilizador
    {
        [Required, StringLength(40)]
        public string NivelAcesso { get; set; } = "moderador";  // ex.: "super", "moderador"

        [DataType(DataType.DateTime)]
        public DateTime DataCriacao { get; set; } = DateTime.Now; // opcional, útil para auditoria
    }
}
