using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_Final.Areas.Identity.Data;
using Microsoft.Extensions.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly IConfiguration _configuration;

        public ResetPasswordConfirmationModel(UserManager<ApplicationUtilisateur> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null)
            {
                var twilioSettings = _configuration.GetSection("TwilioSettings");
                var sendGridSettings = _configuration.GetSection("SendGridSettings");

                // Vérification et initialisation de Twilio
                if (!string.IsNullOrEmpty(user.PhoneNumber))
                {
                    var accountSid = twilioSettings["AccountSId"];
                    var authToken = twilioSettings["AuthToken"];
                    var fromPhoneNumber = twilioSettings["FromPhoneNumber"];

                    TwilioClient.Init(accountSid, authToken);

                    var phoneNumber = user.PhoneNumber;
                    var message = "Votre mot de passe a été changé avec succès.";

                    var twilioMessage = MessageResource.Create(
                        body: message,
                        from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                        to: new Twilio.Types.PhoneNumber(phoneNumber)
                    );

                    if (twilioMessage.Status == MessageResource.StatusEnum.Sent)
                    {
                        // Le message a été envoyé avec succès
                        Console.WriteLine("Statut de l'envoi du SMS: " + twilioMessage.Status.ToString());
                    }
                    else
                    {
                        // Le message n'a pas pu être envoyé
                        Console.WriteLine("Échec de l'envoi du SMS. Statut: " + twilioMessage.Status.ToString());
                    }
                }
                else
                {
                    // Envoi d'un e-mail via SendGrid
                    var apiKey = sendGridSettings["ApiKey"];
                    var senderEmail = sendGridSettings["FromEmail"];
                    var senderName = sendGridSettings["EmailName"];

                    var client = new SendGridClient(apiKey);
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(senderEmail, senderName),
                        Subject = "Notification de changement de mot de passe",
                        PlainTextContent = "Votre mot de passe a été changé avec succès."
                    };
                    msg.AddTo(new EmailAddress(user.Email));

                    var response = await client.SendEmailAsync(msg);

                    if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                    {
                        // L'e-mail a été envoyé avec succès
                        Console.WriteLine("E-mail envoyé avec succès");
                    }
                    else
                    {
                        // Gérer l'échec de l'envoi de l'e-mail
                        Console.WriteLine("Échec de l'envoi de l'e-mail. Statut : " + response.StatusCode);
                    }
                }
            }

            return Page();
        }
    }
}
