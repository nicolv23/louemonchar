using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Projet_Final.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using SendGrid.Helpers.Mail;
using SendGrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Microsoft.AspNetCore.Authorization;

namespace Projet_Final.Areas.Identity.Pages.Account
{

    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly ILogger<LockoutModel> _logger;
        private readonly IEmailSender _emailSender;

        public LockoutModel(IConfiguration configuration, UserManager<ApplicationUtilisateur> userManager, ILogger<LockoutModel> logger, IEmailSender emailSender)
        {
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user != null && _userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
            {
                try
                {
                    var sendGridSettings = _configuration.GetSection("SendGridSettings");
                    var apiKey = sendGridSettings["ApiKey"];
                    var senderEmail = sendGridSettings["FromEmail"];
                    var senderName = sendGridSettings["EmailName"];

                    // Envoi d'un courriel via SendGrid
                    var client = new SendGridClient(apiKey);
                    var msg = new SendGridMessage()
                    {
                        From = new EmailAddress(senderEmail, senderName),
                        Subject = "Notification de verrouillage de compte",
                        PlainTextContent = "Votre compte a été verrouillé pour des mesures de sécurité, veuillez contacter l'administrateur."
                    };
                    msg.AddTo(new EmailAddress(user.Email));

                    var emailResponse = await client.SendEmailAsync(msg);

                    // Envoi d'un SMS via Twilio
                    var twilioSettings = _configuration.GetSection("TwilioSettings");
                    var accountSid = twilioSettings["AccountSId"];
                    var authToken = twilioSettings["AuthToken"];
                    var fromPhoneNumber = twilioSettings["FromPhoneNumber"];

                    TwilioClient.Init(accountSid, authToken);

                    var message = MessageResource.Create(
                        body: "Votre compte a été verrouillé pour des mesures de sécurité, veuillez contacter l'administrateur.",
                        from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                        to: new Twilio.Types.PhoneNumber(user.PhoneNumber)
                    );

                    if (emailResponse.StatusCode == System.Net.HttpStatusCode.Accepted && message != null)
                    {
                        // Le courriel et le SMS ont été envoyés avec succès
                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        // Gérer l'échec de l'envoi du courriel ou du SMS
                        _logger.LogError($"Error sending email. Status code: {emailResponse.StatusCode}");
                        _logger.LogError($"SendGrid Response: {await emailResponse.Body.ReadAsStringAsync()}");
                        _logger.LogError($"Error sending SMS. Status code: {message?.Status}");
                        return Page();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send email or SMS. Error: {ex.Message}");
                    return Page();
                }
            }

            return Page();
        }
    }
}
