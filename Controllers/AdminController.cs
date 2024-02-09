using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projet_Final.Areas.Identity.Data;

namespace Projet_Final.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly SignInManager<ApplicationUtilisateur> _signInManager;

        public AdminController(UserManager<ApplicationUtilisateur> userManager, SignInManager<ApplicationUtilisateur> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string rechercheUtilisateur)
        {
            var utilisateurs = await _userManager.Users.ToListAsync();

            if (utilisateurs != null && !string.IsNullOrEmpty(rechercheUtilisateur))
            {
                utilisateurs = utilisateurs.Where(u =>
                (u.FirstName != null && u.FirstName.Contains(rechercheUtilisateur, StringComparison.OrdinalIgnoreCase)) ||
                (u.LastName != null && u.LastName.Contains(rechercheUtilisateur, StringComparison.OrdinalIgnoreCase)) ||
                (u.UserName != null && u.UserName.Contains(rechercheUtilisateur, StringComparison.OrdinalIgnoreCase)) ||
                (u.Email != null && u.Email.Contains(rechercheUtilisateur, StringComparison.OrdinalIgnoreCase)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(rechercheUtilisateur, StringComparison.OrdinalIgnoreCase))
                ).ToList();

            }
            else if (utilisateurs == null)
            {         
                return View("Aucun utilisateur trouvé");
            }

            return View(utilisateurs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,FirstName,LastName,Email,PhoneNumber")] ApplicationUtilisateur utilisateur)
        {
            if (id != utilisateur.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(id);

                    user.FirstName = utilisateur.FirstName;
                    user.LastName = utilisateur.LastName;
                    user.Email = utilisateur.Email;
                    user.PhoneNumber = utilisateur.PhoneNumber;

                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _userManager.Users.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(utilisateur);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(utilisateur);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(utilisateur);
        }

        [HttpGet]
        public async Task<IActionResult> Verrouiller(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            return View(utilisateur);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerrouillerConfirme(string id)
        {
            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            utilisateur.LockoutEnd = DateTimeOffset.MaxValue;

            var result = await _userManager.UpdateAsync(utilisateur);

            if (result.Succeeded)
            {
                // Facultatif : déconnecter l'utilisateur verrouillé
                await _signInManager.SignOutAsync();

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(utilisateur);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deverrouiller(string id)
        {
            var utilisateur = await _userManager.FindByIdAsync(id);

            if (utilisateur == null)
            {
                return NotFound();
            }

            utilisateur.LockoutEnd = null;

            var result = await _userManager.UpdateAsync(utilisateur);

            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(utilisateur);
        }

    }
}
