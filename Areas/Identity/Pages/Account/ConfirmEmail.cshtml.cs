using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Projet_Final.Areas.Identity.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly ILogger<ConfirmEmailModel> _logger;

        public ConfirmEmailModel(IConfiguration configuration, UserManager<ApplicationUtilisateur> userManager, ILogger<ConfirmEmailModel> logger)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                _logger.LogInformation("User confirmed email successfully.");

                // Utilisation de SendGrid pour envoyer un e-mail de remerciement
                var sendGridSettings = _configuration.GetSection("SendGridSettings");
                var apiKey = sendGridSettings["ApiKey"];
                var senderEmail = sendGridSettings["FromEmail"];
                var senderName = sendGridSettings["EmailName"];

                var client = new SendGridClient(apiKey);
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(senderEmail, senderName),
                    Subject = "Merci d'avoir confirmé votre email",
                    PlainTextContent = "Merci d'avoir confirmé votre email sur notre plateforme."
                };
                msg.AddTo(new EmailAddress(user.Email));

                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                {
                    // Envoi du SMS
                    var twilioSettings = _configuration.GetSection("TwilioSettings");
                    var accountSid = twilioSettings["AccountSId"];
                    var authToken = twilioSettings["AuthToken"];
                    var twilioPhoneNumber = twilioSettings["FromPhoneNumber"];

                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        body: "Merci d'avoir confirmé votre email sur notre plateforme.",
                        from: new Twilio.Types.PhoneNumber(twilioPhoneNumber),
                        to: new Twilio.Types.PhoneNumber(user.PhoneNumber) 
                    );

                    // Redirection vers la page d'accueil ou une autre page après confirmation
                    return RedirectToPage("/Index");
                }
                else
                {
                    // Gérer l'échec de l'envoi du courriel
                    _logger.LogError($"Error sending email. Status code: {response.StatusCode}");
                    _logger.LogError($"SendGrid Response: {await response.Body.ReadAsStringAsync()}");
                    return Page();
                }
            }
            else
            {
                // Gérer l'échec de la confirmation de l'email
                _logger.LogError("Error confirming email.");
                return Page();
            }
        }
    }
}
