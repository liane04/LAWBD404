using System.Threading.Tasks;

namespace Marketplace.Services
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody);
    }
}

