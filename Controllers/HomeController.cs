using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Projet_Final.Areas.Identity.Data;
using Projet_Final.Models;
using System.Diagnostics;

namespace Projet_Final.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUtilisateur> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUtilisateur> userManager, ApplicationDbContext dbContext)
        {
            _logger = logger;
            this._userManager = userManager;
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {
            // Récupérez les données de la table Voitures
            var voitures = _dbContext.Voitures.ToList();

            // Passez les données de voiture et l'ID de l'utilisateur à la vue
            ViewData["Voitures"] = voitures;
            ViewData["UtilisateurID"] = _userManager.GetUserId(this.User);

            return View();
        }

        public IActionResult ListeModeles()
        {
            var voitures = _dbContext.Voitures.ToList();
            return View(voitures);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}