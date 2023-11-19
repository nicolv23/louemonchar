

using Microsoft.Extensions.Options;
using Projet_Final.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Projet_Final.Services
{
    public class SMSSenderService : ISMSSenderService
    {
        private readonly TwilioSettings _twilioSettings; // Paramètres de configuration Twilio
                                                         // Constructeur de la classe
        public SMSSenderService(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value; // Récupère les paramètres de configuration Twilio depuis les options
                                                    // Le constructeur prend en argument des options de configuration Twilio.
        }
        // Méthode pour envoyer un SMS de manière asynchrone
        public async Task SendSmsAsync(string number, string message)
        {
            // Initialise le client Twilio en utilisant les informations d'identification de TwilioSettings
            TwilioClient.Init(_twilioSettings.AccountSId, _twilioSettings.AuthToken);
            // Crée un message SMS en utilisant Twilio et envoie le message
            await MessageResource.CreateAsync(
            to: number, // Numéro de téléphone du destinataire
            from: _twilioSettings.FromPhoneNumber, // Numéro de téléphone émetteur configuré dans TwilioSettings
            body: message // Corps du message SMS
            );
        }
    }
}
