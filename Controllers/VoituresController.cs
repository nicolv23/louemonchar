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

        public VoituresController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create([Bind("Id,Marque,Modèle,Année,PrixJournalier,EstDisponible")] Voiture voiture, IFormFile imageVoiture)
        {
            if (ModelState.IsValid)
            {
                if (imageVoiture != null && imageVoiture.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageVoiture.FileName);

                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageVoiture.CopyToAsync(fileStream);
                    }
                    voiture.ImageVoiture = "/images/voitures" + fileName; 
                }

                _context.Add(voiture);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(voiture);
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,Marque,Modèle,Année,PrixJournalier,EstDisponible")] Voiture voiture)
        {
            if (id != voiture.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voiture);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(voiture);
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
