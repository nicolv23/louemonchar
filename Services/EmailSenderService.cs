using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Projet_Final.Settings;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Projet_Final.Services
{
    //La classe EmailSenderService qui implémente une interface IEmailSender pour l'envoi d'e-mails.
    //Il utilise la bibliothèque SendGrid pour envoyer des e-mails.
    public class EmailSenderService : IEmailSender
    {
        // Champs privés pour stocker des dépendances et des paramètres
        private readonly ISendGridClient _sendGridClient; // Client SendGrid pour l'envoi d'e-mails
        private readonly SendGridSettings _sendGridSettings; // Paramètres de configuration pour SendGrid
        private readonly ILogger _logger; // Logger pour enregistrer des informations
                                          // Constructeur de la classe
        public EmailSenderService(ISendGridClient sendGridClient, IOptions<SendGridSettings> sendGridSettings, ILogger<IEmailSender> logger)
        {
            _sendGridClient = sendGridClient; // Initialise le client SendGrid
            _sendGridSettings = sendGridSettings.Value; // Récupère les paramètres de configuration de SendGrid depuis les options
            _logger = logger; // Initialise le logger
                              // Le constructeur prend en argument un client SendGrid, des options de configuration SendGrid, et un logger.
        }
        // Méthode pour envoyer un e-mail de manière asynchrone
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Crée un objet SendGridMessage pour représenter l'e-mail
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridSettings.FromEmail, _sendGridSettings.EmailName), // Définit l'expéditeur
                Subject = subject, // Définit le sujet de l'e-mail
                HtmlContent = htmlMessage // Définit le contenu HTML de l'e-mail
            };
            msg.AddTo(email); // Ajoute le destinataire de l'e-mail
                              // Envoie l'e-mail en utilisant le client SendGrid et attend une réponse
            var response = await _sendGridClient.SendEmailAsync(msg);
            // Enregistre des informations de journalisation (logs) en fonction de la réussite de l'envoi de l'e-mail
            _logger.LogInformation(response.IsSuccessStatusCode
            ? $"Email to {email} queued successfully!"
            : $"Failure Email to {email}");
        }
    }
}