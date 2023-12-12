// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Projet_Final.Areas.Identity.Data;
using System.Net.Mail;
using SendGrid.Helpers.Mail;
using SendGrid;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUtilisateur> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;

        public LoginModel(SignInManager<ApplicationUtilisateur> signInManager, ILogger<LoginModel> logger, IEmailSender emailSender,
            IConfiguration configuration)
        {
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _configuration = configuration;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Se souvenir de moi?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                if (user != null)
                {

                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation("User logged in.");
                        return LocalRedirect(returnUrl);
                    }
                    else if (result.IsNotAllowed)
                    {
                        ModelState.AddModelError(string.Empty, "Veuillez confirmer votre adresse e-mail.");
                    }
                    else if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }
                    else if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User account locked out.");

                        // Récupérez l'utilisateur bloqué
                        user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);

                        // Envoi d'un courriel via SendGrid lorsque le compte est bloqué
                        var sendGridSettings = _configuration.GetSection("SendGridSettings");
                        var apiKey = sendGridSettings["ApiKey"];
                        var senderEmail = sendGridSettings["FromEmail"];
                        var senderName = sendGridSettings["EmailName"];

                        var client = new SendGridClient(apiKey);
                        var emailMsg = new SendGridMessage()
                        {
                            From = new EmailAddress(senderEmail, senderName),
                            Subject = "Notification de verrouillage de compte",
                            PlainTextContent = "Votre compte a été verrouillé pour des raisons de sécurité. Veuillez contacter l'administrateur."
                        };
                        emailMsg.AddTo(new EmailAddress(user.Email));

                        var response = await client.SendEmailAsync(emailMsg);

                        if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
                        {
                            // L'email a été envoyé avec succès
                            _logger.LogInformation("Email envoyé avec succès.");
                        }
                        else
                        {
                            // Gérer l'échec de l'envoi de l'email
                            _logger.LogError($"Échec de l'envoi de l'email. Code d'état : {response.StatusCode}");
                        }

                        // Envoi d'un SMS via Twilio lorsque le compte est bloqué
                        if (!string.IsNullOrEmpty(user.PhoneNumber))
                        {
                            var twilioSettings = _configuration.GetSection("TwilioSettings");
                            var accountSid = twilioSettings["AccountSId"];
                            var authToken = twilioSettings["AuthToken"];
                            var fromPhoneNumber = twilioSettings["FromPhoneNumber"];

                            TwilioClient.Init(accountSid, authToken);

                            var phoneNumber = user.PhoneNumber;
                            var message = "Votre compte a été verrouillé pour des raisons de sécurité. Veuillez contacter l'administrateur.";

                            var twilioMessage = MessageResource.Create(
                                body: message,
                                from: new Twilio.Types.PhoneNumber(fromPhoneNumber),
                                to: new Twilio.Types.PhoneNumber(phoneNumber)
                            );

                            if (twilioMessage.Status == MessageResource.StatusEnum.Sent)
                            {
                                // Le message a été envoyé avec succès
                                _logger.LogInformation("SMS envoyé avec succès.");

                            }
                            else
                            {
                                // Le message n'a pas pu être envoyé
                                _logger.LogError("Échec de l'envoi du SMS.");

                            }
                        }

                        return RedirectToPage("./Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Le mot de passe ne correspond pas à l'adresse e-mail.");
                    }
                }
            }

            // Si le modèle n'est pas valide ou si des erreurs se sont produites, retourner la page avec les erreurs de validation
            return Page();
        }
    }
}
