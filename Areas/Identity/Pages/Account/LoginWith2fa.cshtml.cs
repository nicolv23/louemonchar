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
        private readonly IEmailSender _emailSender;
        private readonly ISMSSenderService _sMSSenderService; // Ajouter le service ISMSSenderService ici

        public LoginWith2faModel(
            SignInManager<ApplicationUtilisateur> signInManager,
            UserManager<ApplicationUtilisateur> userManager,
            ILogger<LoginWith2faModel> logger,
            IEmailSender emailSender,
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

        public async Task<IActionResult> OnGetAsync(bool rememberMe, string returnUrl = null)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            Input = new InputModel();

            if (providers.Any(_ => _ == "Phone"))
            {
                Input.TwoFactAuthProviderName = "Phone";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Phone");
                // Envoyer le token par SMS à l'utilisateur via _sMSSenderService
                await _sMSSenderService.SendSmsAsync(user.PhoneNumber, $"OTP Code: {token}");
            }
            else if (providers.Any(_ => _ == "Email"))
            {
                Input.TwoFactAuthProviderName = "Email";
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                // Envoyer le token par e-mail à l'utilisateur via _emailSender
                await _emailSender.SendEmailAsync(user.Email, "2FA Code", $"<h3>{token}</h3>.");
            }
            else
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(bool rememberMe, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            returnUrl = returnUrl ?? Url.Content("~/");
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);
            var result = await _signInManager.TwoFactorSignInAsync(Input.TwoFactAuthProviderName, authenticatorCode, rememberMe, Input.RememberMachine);
            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2FA.", user.Id);
                return LocalRedirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return RedirectToPage("./Lockout");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
            }

            return Page();
        }
    }
}
