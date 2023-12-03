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

        public AdminController(UserManager<ApplicationUtilisateur> userManager)
        {
            _userManager = userManager;
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
    }
}
