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
            var sendGridSettings = _configuration.GetSection("SendGridSettings");
            var smtpServer = sendGridSettings["SmtpServer"];
            // Vérification et récupération de la valeur du port avec une gestion d'erreur
            var portValue = sendGridSettings["Port"];
            int port;
            if (!int.TryParse(portValue, out port))
            {
                // Gérer l'erreur si la valeur du port n'est pas valide
                // Par exemple, retourner une vue avec un message d'erreur
                return Content("Erreur de configuration du port SMTP");
            }
            var userName = sendGridSettings["UserName"];
            var password = sendGridSettings["Password"];
            var senderEmail = sendGridSettings["SenderEmail"];
            var senderName = sendGridSettings["SenderName"];

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

                // Envoi du SMS de confirmation via Twilio
                var twilioSettings = _configuration.GetSection("TwilioSettings");
                var accountSid = twilioSettings["AccountSId"];
                var authToken = twilioSettings["AuthToken"];
                var fromPhoneNumber = twilioSettings["FromPhoneNumber"];

                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: "Merci d'avoir confirmé votre email.",
                    from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                    to: new Twilio.Types.PhoneNumber(user.PhoneNumber));

                Console.WriteLine($"Twilio Message SID: {message.Sid}");
                Console.WriteLine($"Twilio Message Error Message: {message.ErrorMessage}");

                // Redirection vers la page d'accueil ou une autre page après confirmation
                return RedirectToPage("/Index");
            }
            else
            {
                _logger.LogError("Error confirming email.");
                return Page(); // Affichez un message d'erreur sur la page de confirmation d'email.
            }
        }
    }
}
