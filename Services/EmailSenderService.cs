using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projet_Final.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace Projet_Final.Services
{
    public class EmailSenderService : IEmailSender
    {
        private readonly ISendGridClient _sendGridClient;
        private readonly SendGridSettings _sendGridSettings;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(ISendGridClient sendGridClient, IOptions<SendGridSettings> sendGridSettings, ILogger<EmailSenderService> logger)
        {
            _sendGridClient = sendGridClient;
            _sendGridSettings = sendGridSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.EmailName),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            msg.AddTo(email);

            var response = await _sendGridClient.SendEmailAsync(msg);

            _logger.LogInformation(response.IsSuccessStatusCode ? $"Email to {email} queued successfully!" : $"Failure Email to {email}");
        }
    }
}
