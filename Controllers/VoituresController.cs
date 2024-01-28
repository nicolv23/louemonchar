using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projet_Final.Areas.Identity.Data;
using Projet_Final.Models;

namespace Projet_Final.Controllers
{
    public class VoituresController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<VoituresController> _logger;

        public VoituresController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<VoituresController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger; 
        }

        [HttpGet]
        // GET: Voitures
        public async Task<IActionResult> Index(string recherche)
        {
            ViewData["Title"] = "Index";
            IQueryable<Voiture> voitures = _context.Voitures;

            if (!string.IsNullOrEmpty(recherche))
            {
                voitures = voitures.Where(v =>
                    EF.Functions.Like(v.Marque, $"%{recherche}%") ||
                    EF.Functions.Like(v.Modèle, $"%{recherche}%") ||
                    EF.Functions.Like(v.Année.ToString(), $"%{recherche}%") ||
                    EF.Functions.Like(v.PrixJournalier.ToString(), $"%{recherche}%") ||
                    EF.Functions.Like(v.EstDisponible.ToString(), $"%{recherche}%")
                );
            }

            ViewData["RechercheValue"] = recherche; // Passer la valeur de recherche à la vue

            var voituresList = await voitures.ToListAsync(); // Récupérer les résultats de la recherche

            return View(voituresList); // Passer la liste filtrée à la vue
        }

        // GET: Voitures/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Voitures == null)
            {
                return NotFound();
            }

            var voiture = await _context.Voitures
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voiture == null)
            {
                return NotFound();
            }

            return View(voiture);
        }

        [HttpGet]
        public IActionResult GetDetails(int id)
        {
            var voiture = _context.Voitures.FirstOrDefault(v => v.Id == id);

            if (voiture == null)
            {
                return NotFound();
            }

            return Json(new { Marque = voiture.Marque, Modele = voiture.Modèle });
        }

        // GET: Voitures/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Voitures/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Marque,Modèle,Année,PrixJournalier,EstDisponible,ImageVoiture")] Voiture voiture, IFormFile imageVoiture)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageVoiture != null && imageVoiture.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(imageVoiture.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageVoiture", "Seules les images avec les extensions .jpg, .jpeg, .png et .gif sont autorisées.");
                            return View(voiture);
                        }

                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageVoiture.FileName);
                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "voitures", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageVoiture.CopyToAsync(fileStream);
                        }

                        voiture.ImageVoiture = Path.Combine("images", "voitures", fileName);
                    }

                    _context.Add(voiture);
                    await _context.SaveChangesAsync();

                    return Json(new { redirectUrl = Url.Action("Index") });                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur lors de la sauvegarde de la voiture");
                    ModelState.AddModelError(string.Empty, "Une erreur s'est produite lors de la sauvegarde de la voiture.");
                }
            }

            // Si ModelState.IsValid n'est pas vrai ou s'il y a une exception, retournez les erreurs
            var errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray());
            return Json(new { errors });
        }

        // GET: Voitures/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Voitures == null)
            {
                return NotFound();
            }

            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture == null)
            {
                return NotFound();
            }
            return View(voiture);
        }

        // POST: Voitures/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Marque,Modèle,Année,PrixJournalier,EstDisponible,ImageVoiture")] Voiture voiture, IFormFile nouvelleImageVoiture)
        {
            if (id != voiture.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (nouvelleImageVoiture != null && nouvelleImageVoiture.Length > 0)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExtension = Path.GetExtension(nouvelleImageVoiture.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("ImageVoiture", "Seules les images avec les extensions .jpg, .jpeg, .png et .gif sont autorisées.");
                            return View(voiture);
                        }

                        var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(nouvelleImageVoiture.FileName);

                        var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "images", "voitures", fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await nouvelleImageVoiture.CopyToAsync(fileStream);
                        }

                        // Supprimer l'ancienne image si elle existe
                        if (!string.IsNullOrEmpty(voiture.ImageVoiture))
                        {
                            var ancienChemin = Path.Combine(_hostingEnvironment.WebRootPath, voiture.ImageVoiture);
                            if (System.IO.File.Exists(ancienChemin))
                            {
                                System.IO.File.Delete(ancienChemin);
                            }
                        }

                        voiture.ImageVoiture = Path.Combine("images", "voitures", fileName);
                    }

                    _context.Update(voiture);
                    await _context.SaveChangesAsync();
                    return Json(new { redirectUrl = Url.Action("Index") });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoitureExists(voiture.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (IOException ioEx)
                {
                    _logger.LogError(ioEx, "Erreur d'entrée/sortie lors de la copie du fichier");
                    ModelState.AddModelError(string.Empty, "Erreur d'entrée/sortie lors de la copie du fichier.");
                    return Json(new { error = "Erreur d'entrée/sortie lors de la copie du fichier." });
                }
                catch (UnauthorizedAccessException authEx)
                {
                    _logger.LogError(authEx, "Erreur d'autorisation lors de la copie du fichier");
                    ModelState.AddModelError(string.Empty, "Erreur d'autorisation lors de la copie du fichier.");
                    return Json(new { error = "Erreur d'autorisation lors de la copie du fichier." });
                }
            }

            return Json(new { errors = ModelState.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()) });
        }

        // GET: Voitures/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Voitures == null)
            {
                return NotFound();
            }

            var voiture = await _context.Voitures
                .FirstOrDefaultAsync(m => m.Id == id);
            if (voiture == null)
            {
                return NotFound();
            }

            return View(voiture);
        }

        // POST: Voitures/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Voitures == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Voitures'  is null.");
            }
            var voiture = await _context.Voitures.FindAsync(id);
            if (voiture != null)
            {
                _context.Voitures.Remove(voiture);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Viewpayer()
        {
            return View("payer");

        }

        private bool VoitureExists(int id)
        {
          return (_context.Voitures?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
