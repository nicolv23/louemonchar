// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Projet_Final.Areas.Identity.Data;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterConfirmationModel : PageModel
    {
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly IEmailSender _sender;

        public RegisterConfirmationModel(UserManager<ApplicationUtilisateur> userManager, IEmailSender sender)
        {
            _userManager = userManager;
            _sender = sender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            if (email == null)
            {
                return RedirectToPage("/Index");
            }
            returnUrl = returnUrl ?? Url.Content("~/");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return NotFound($"Unable to load user with email '{email}'.");
            }

            Email = email;

            // Envoyer un message de bienvenue à LoueMonChar
            await EnvoyerCourrielBienvenue(email);

            return Page();
        }

        private async Task EnvoyerCourrielBienvenue(string email)
        {
            var apiKey = "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";

            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("nicolaslv0018@gmail.com", "Nicolas Lazarte"),
                Subject = "Bienvenue sur LoueMonChar",
                PlainTextContent = "Bienvenue sur LoueMonChar! Merci de vous être inscrit."
            };
            msg.AddTo(new EmailAddress(email));

            var response = await client.SendEmailAsync(msg);

            // Gérer la réponse de l'envoi de l'e-mail
            if (response.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                // L'e-mail a été envoyé avec succès
                Console.WriteLine("L'e-mail de bienvenue a été envoyé avec succès à : " + email);
                // Ajoutez ici d'autres actions en cas de succès
            }
            else
            {
                // Gérer l'échec de l'envoi de l'e-mail
                Console.WriteLine("Échec de l'envoi de l'e-mail de bienvenue à : " + email);
                // Ajoutez ici d'autres actions en cas d'échec
            }
        }

    }
}
