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
            var emailSettings = _configuration.GetSection("EmailSettings");
            var smtpServer = emailSettings["SmtpServer"];
            var port = int.Parse(emailSettings["Port"]);
            var userName = emailSettings["UserName"];
            var password = emailSettings["Password"];
            var senderEmail = emailSettings["SenderEmail"];
            var senderName = emailSettings["SenderName"];

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

                // Envoi de l'email de confirmation
                using (var client = new SmtpClient(smtpServer))
                {
                    client.Port = port;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(userName, password);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = "Confirmation d'email",
                        Body = "Merci d'avoir confirmé votre email."
                    };

                    mailMessage.To.Add(user.Email);

                    client.Send(mailMessage);
                }

                return RedirectToPage("/Index"); // Redirigez vers la page d'accueil ou une autre page après confirmation.
            }
            else
            {
                _logger.LogError("Error confirming email.");
                return Page(); // Affichez un message d'erreur sur la page de confirmation d'email.
            }
        }
    }
}
