using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_Final.Areas.Identity.Data;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly IConfiguration _configuration;

        public ResetPasswordModel(UserManager<ApplicationUtilisateur> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Le champ mot de passe est requis.")]
            [StringLength(100, ErrorMessage = "Le mot de passe doit contenir entre {2} et {1} caractères.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmer le mot de passe")]
            [Compare("Password", ErrorMessage = "Le mot de passe et sa confirmation ne correspondent pas.")]
            public string ConfirmPassword { get; set; }

            public string Code { get; set; }
        }

        public IActionResult OnGet(string code = null)
        {
            if (code == null)
            {
                return BadRequest("Un code doit être fourni pour la réinitialisation du mot de passe.");
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code))
                };
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Utilisateur non trouvé. Veuillez vérifier l'adresse e-mail.");
                return Page(); // Retourne la page actuelle avec un message d'erreur
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                await NotifierChangementMDP(user); // Envoi d'e-mails et de SMS de confirmation
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        private async Task NotifierChangementMDP(ApplicationUtilisateur user)
        {
            var sendGridSettings = _configuration.GetSection("SendGridSettings");
            var twilioSettings = _configuration.GetSection("TwilioSettings");

            var apiKey = sendGridSettings["ApiKey"];
            var senderEmail = sendGridSettings["FromEmail"];
            var senderName = sendGridSettings["EmailName"];

            var client = new SendGridClient(apiKey);
            var emailMsg = new SendGridMessage()
            {
                From = new EmailAddress(senderEmail, senderName),
                Subject = "Changement de mot de passe réussi",
                PlainTextContent = "Votre mot de passe a été changé avec succès."
            };
            emailMsg.AddTo(new EmailAddress(user.Email));

            var response = await client.SendEmailAsync(emailMsg);

            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // L'e-mail a été envoyé avec succès
                // Ajoutez ici le code pour gérer le succès de l'envoi de l'e-mail si nécessaire
            }
            else
            {
                // Gérer l'échec de l'envoi de l'e-mail
                // Ajoutez ici le code pour gérer l'échec de l'envoi de l'e-mail si nécessaire
            }

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
                    // Ajoutez ici le code pour gérer le succès de l'envoi du SMS si nécessaire
                }
                else
                {
                    // Le message n'a pas pu être envoyé
                    // Ajoutez ici le code pour gérer l'échec de l'envoi du SMS si nécessaire
                }
            }
        }
    }
}
