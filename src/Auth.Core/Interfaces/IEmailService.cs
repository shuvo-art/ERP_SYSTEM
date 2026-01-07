using System.Threading.Tasks;

namespace Auth.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendPasswordResetEmailAsync(string to, string token);
    }
}
