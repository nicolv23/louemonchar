using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Projet_Final.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Projet_Final.Services;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    public class LoginWith2faModel : PageModel
    {
        private readonly SignInManager<ApplicationUtilisateur> _signInManager;
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly ILogger<LoginWith2faModel> _logger;
        private readonly Microsoft.AspNetCore.Identity.UI.Services.IEmailSender _emailSender;
        private readonly ISMSSenderService _sMSSenderService; // Ajouter le service ISMSSenderService ici

        public LoginWith2faModel(
            SignInManager<ApplicationUtilisateur> signInManager,
            UserManager<ApplicationUtilisateur> userManager,
            ILogger<LoginWith2faModel> logger,
            Microsoft.AspNetCore.Identity.UI.Services.IEmailSender emailSender,
            ISMSSenderService sMSSenderService) // Ajouter ISMSSenderService dans le constructeur
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _sMSSenderService = sMSSenderService; // Initialiser l'instance de ISMSSenderService
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public bool RememberMe { get; set; }
        public string ReturnUrl { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
            public string TwoFactAuthProviderName { get; set; }
        }

        //Ce méthode est utilisé pour envoyer des jetons à deux facteurs
        //à l'utilisateur en fonction
        //du fournisseur d'authentification disponible (téléphone ou e-mail)
        //et de la demande de l'utilisateur.
        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.UserManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("./Login", new { returnUrl });
            }

            var isTwoFactorAuthenticated = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (isTwoFactorAuthenticated == null)
            {
                throw new InvalidOperationException($"Impossible de charger l'utilisateur d'authentification à deux facteurs.");
            }

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            Input = new InputModel();

            if (providers.Any(_ => _ == "Phone"))
            {
                Input.TwoFactAuthProviderName = "Phone";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
                // Envoie le code par SMS
                await _sMSSenderService.SendSmsAsync(user.PhoneNumber, $"Votre code de vérification: {token}");
            }
            else if (providers.Any(_ => _ == "Email"))
            {
                Input.TwoFactAuthProviderName = "Email";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                // Envoie le code par e-mail
                await _emailSender.SendEmailAsync(user.Email, "Code de vérification à deux facteurs", $"<h3>Votre code de vérification: {token}</h3>.");
            }
            else
            {
                throw new InvalidOperationException($"Impossible de charger l'utilisateur d'authentification à deux facteurs.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;

            return Page();
        }

        // Cette méthode fait partie d'une action de gestion
        // d'une page web (une page d'authentification à deux facteurs)
        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            // Vérifie si le modèle (ModelState) est valide, c'est-à-dire s'il n'y a pas d'erreurs de validation
            if (!ModelState.IsValid)
            {
                return Page(); // Recharge la page actuelle si des erreurs de validation sont détectées
            }
            // Si returnUrl est nul, le remplace par l'URL par défaut
            returnUrl = returnUrl ?? Url.Content("~/");
            // Récupère l'utilisateur qui a passé par l'authentification à deux facteurs
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Impossible de charger l'utilisateur d'authentification à deux facteurs.");
            }
            // Récupère le code d'authentification à deux facteurs de l'entrée (Input) en supprimant les espaces et les tirets
            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            // Tente de valider le code d'authentification à deux facteurs en utilisant le gestionnaire d'authentification
            var result = await _signInManager.TwoFactorSignInAsync(Input.TwoFactAuthProviderName, authenticatorCode, rememberMe, Input.RememberMachine);
            // Récupère l'ID de l'utilisateur
            var userId = await _userManager.GetUserIdAsync(user);
            if (result.Succeeded)
            {
                // L'authentification à deux facteurs a réussi, enregistre un message de journalisation
                _logger.LogInformation("L'utilisateur avec l'ID '{UserId}' s'est connecté avec 2FA.", user.Id);
                // Redirige l'utilisateur vers l'URL de retour spécifiée
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                // Le compte de l'utilisateur est verrouillé, enregistre un message de journalisation
                _logger.LogWarning("Le compte de l'utilisateur avec l'ID '{UserId}' est verrouillé.", user.Id);
                // Redirige l'utilisateur vers une page de verrouillage de compte
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Le code d'authentification à deux facteurs est incorrect, enregistre un message de journalisation
                _logger.LogWarning("Code d'authentification incorrect pour l'utilisateur avec l'ID '{UserId}'.", user.Id);

                return Page();
            }
        }
    }
}
