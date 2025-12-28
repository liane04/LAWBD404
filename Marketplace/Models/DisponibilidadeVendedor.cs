using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    /// <summary>
    /// Representa os horários disponíveis de um vendedor para visitas
    /// </summary>
    public class DisponibilidadeVendedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int VendedorId { get; set; }
        [ForeignKey("VendedorId")]
        public Vendedor Vendedor { get; set; } = null!;

        /// <summary>
        /// Dia da semana (0 = Domingo, 1 = Segunda, ..., 6 = Sábado)
        /// </summary>
        [Required]
        [Range(0, 6)]
        public int DiaSemana { get; set; }

        /// <summary>
        /// Hora de início (ex: 09:00)
        /// </summary>
        [Required]
        public TimeSpan HoraInicio { get; set; }

        /// <summary>
        /// Hora de fim (ex: 18:00)
        /// </summary>
        [Required]
        public TimeSpan HoraFim { get; set; }

        /// <summary>
        /// Intervalo entre visitas em minutos (ex: 30, 60)
        /// </summary>
        [Required]
        [Range(15, 240)]
        public int IntervaloMinutos { get; set; } = 60;

        /// <summary>
        /// Se esta disponibilidade está ativa
        /// </summary>
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Propriedade auxiliar para exibir o nome do dia
        [NotMapped]
        public string NomeDia => DiaSemana switch
        {
            0 => "Domingo",
            1 => "Segunda-feira",
            2 => "Terça-feira",
            3 => "Quarta-feira",
            4 => "Quinta-feira",
            5 => "Sexta-feira",
            6 => "Sábado",
            _ => "Desconhecido"
        };
    }
}
