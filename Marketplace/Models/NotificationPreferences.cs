using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Marketplace.Models
{
    /// <summary>
    /// Preferências de notificação do utilizador
    /// </summary>
    public class NotificationPreferences
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID do Identity User
        /// </summary>
        [Required]
        public string IdentityUserId { get; set; } = null!;

        /// <summary>
        /// Receber notificações por email
        /// </summary>
        public bool EmailNotifications { get; set; } = true;

        /// <summary>
        /// Receber alertas de novos anúncios que correspondam às preferências
        /// </summary>
        public bool NewListingsAlerts { get; set; } = true;

        /// <summary>
        /// Receber alertas quando veículos favoritos baixarem de preço
        /// </summary>
        public bool PriceDropAlerts { get; set; } = false;

        /// <summary>
        /// Receber newsletter com dicas e ofertas
        /// </summary>
        public bool Newsletter { get; set; } = true;

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
