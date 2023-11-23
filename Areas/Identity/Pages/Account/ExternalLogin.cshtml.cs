using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Projet_Final.Areas.Identity.Data;

namespace Projet_Final.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUtilisateur> _signInManager;
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly IUserStore<ApplicationUtilisateur> _userStore;
        private readonly IUserEmailStore<ApplicationUtilisateur> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUtilisateur> signInManager,
            UserManager<ApplicationUtilisateur> userManager,
            IUserStore<ApplicationUtilisateur> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
        ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
        ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
        /// </summary>
        public string ProviderDisplayName { get; set; }

        /// <summary>
        ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
        ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
        ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
        ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     Cette API prend en charge l'infrastructure UI par défaut d'ASP.NET Core Identity et n'est pas destinée à être utilisée
            ///     directement à partir de votre code. Cette API peut changer ou être supprimée dans les versions futures.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required(ErrorMessage = "Veuillez saisir votre prénom")]
            [Display(Name = "Prénom")]
            public string FirstName { get; set; }

            [Required(ErrorMessage = "Veuillez saisir votre nom de famille")]
            [Display(Name = "Nom de famille")]
            public string LastName { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Demande une redirection vers le fournisseur d'authentification externe.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Erreur du fournisseur externe : {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Erreur lors du chargement des informations d'authentification externe.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            // Connecte l'utilisateur avec ce fournisseur d'authentification externe s'il possède déjà un compte.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} connecté avec le fournisseur {LoginProvider}.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }
            else
            {
                // Si l'utilisateur n'a pas de compte, lui demande de créer un compte.
                ReturnUrl = returnUrl;
                ProviderDisplayName = info.ProviderDisplayName;
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Input = new InputModel
                    {
                        Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                    };
                }
                return Page();
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            // Obtient les informations sur l'utilisateur depuis le fournisseur d'authentification externe.
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Erreur lors du chargement des informations d'authentification externe lors de la confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Utilisateur a créé un compte en utilisant le fournisseur {Name}.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new { area = "Identity", userId = userId, code = code },
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirmation de votre courriel pour l'authentification externe",
                            $"Veuillez confirmer votre compte d'authentification externe en cliquant <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>cliquant ici</a>.");

                        // Si la confirmation du compte est requise, nous devons afficher le lien si nous n'avons pas de véritable expéditeur d'e-mail
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
            return Page();
        }

        private ApplicationUtilisateur CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUtilisateur>();
            }
            catch
            {
                throw new InvalidOperationException($"Impossible de créer une instance de '{nameof(ApplicationUtilisateur)}'. " +
                    $"Assurez-vous que '{nameof(ApplicationUtilisateur)}' n'est pas une classe abstraite et possède un constructeur sans paramètre, ou bien " +
                    $"remplacez la page d'authentification externe dans /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUtilisateur> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("L'interface utilisateur par défaut nécessite un magasin d'utilisateurs avec prise en charge de l'e-mail.");
            }
            return (IUserEmailStore<ApplicationUtilisateur>)_userStore;
        }
    }
}
