using System.Net;
using System.Net.Mail;
using Auth.Core.Interfaces;
using Auth.Core.Settings;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public SmtpEmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var client = new SmtpClient(_emailSettings.SmtpHost, _emailSettings.SmtpPort)
                {
                    EnableSsl = _emailSettings.EnableSsl,
                    Credentials = new NetworkCredential(_emailSettings.SmtpUser, _emailSettings.SmtpPass)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.FromEmail, _emailSettings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
            }
            catch (System.Exception ex)
            {
                // In a production app, we would log this error properly
                System.Console.WriteLine($"Failed to send email: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string to, string token)
        {
            var subject = "Password Reset Request";
            var body = $@"
                <h3>Password Reset Request</h3>
                <p>You requested to reset your password. Please use the following token to reset it:</p>
                <div style='background-color: #f4f4f4; padding: 15px; font-weight: bold; font-family: monospace; border-radius: 5px;'>
                    {token}
                </div>
                <p>This token will expire in 1 hour.</p>
                <p>If you did not request this, please ignore this email.</p>";

            await SendEmailAsync(to, subject, body);
        }
    }
}
