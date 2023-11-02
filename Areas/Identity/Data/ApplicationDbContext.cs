using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Projet_Final.Areas.Identity.Data;
using Projet_Final.Models;
using System;
using System.Linq;

namespace Projet_Final.Areas.Identity.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUtilisateur>
    {
        private readonly TestData _testData;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, TestData testData)
            : base(options)
        {
            _testData = testData;
        }

        // Ajoutez d'autres DbSet pour vos entités si nécessaire
        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Client> Clients { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Voiture>()
                .Property(v => v.Id)
                .ValueGeneratedOnAdd();

            builder.Entity<Client>()
                .Property(c => c.ClientId)
                .ValueGeneratedOnAdd();

            builder.Entity<Reservation>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd();

            // Configuration des relations
            builder.Entity<Reservation>()
                .HasOne(r => r.Voiture)
                .WithMany()
                .HasForeignKey(r => r.VoitureId);

            builder.Entity<Reservation>()
                .HasOne(r => r.Client)
                .WithMany()
                .HasForeignKey(r => r.ClientId);

            // Spécifiez le type de données pour la propriété PrixJournalier dans l'entité Voiture
            builder.Entity<Voiture>()
                .Property(v => v.PrixJournalier)
                .HasColumnType("decimal(18, 2)");
        }


        public static void SeedData(ApplicationDbContext context)
        {
            try
            {
                var testData = new TestData();
                var voitures = testData.GetVoitures();
                var clients = testData.GetClients();
                var reservations = testData.GetReservations(voitures, clients);

                if (!context.Voitures.Any())
                {
                    context.Voitures.AddRange(voitures);
                    context.SaveChanges();
                }

                if (!context.Clients.Any())
                {
                    context.Clients.AddRange(clients);
                    context.SaveChanges();
                }

                // Récupérez les clients depuis la base de données pour obtenir les ID réels
                var clientsFromDb = context.Clients.ToList();
                var voituresFromDb = context.Voitures.ToList();

                // Assurez-vous que chaque réservation a un client valide
                foreach (var reservation in reservations)
                {
                    var client = clientsFromDb.FirstOrDefault(c =>
                        c.Nom == reservation.Client.Nom &&
                        c.Prénom == reservation.Client.Prénom &&
                        c.Adresse == reservation.Client.Adresse &&
                        c.NuméroTéléphone == reservation.Client.NuméroTéléphone);

                    if (reservation.Voiture != null)
                    {
                        var voiture = voituresFromDb.FirstOrDefault(v => v.Id == reservation.Voiture.Id);

                        if (client != null && voiture != null)
                        {
                            reservation.Client = client; 
                            reservation.Voiture = voiture; 
                            reservation.ClientId = client.ClientId; 
                            reservation.VoitureId = voiture.Id; 
                        }
                    }
                }


                if (!context.Reservations.Any())
                {
                    context.Reservations.AddRange(reservations);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Affichez l'exception interne dans la console ou les journaux pour déboguer.
                Console.WriteLine($"Erreur lors de l'ajout des données : {ex.InnerException}");
                throw; // Rejetez l'exception pour qu'elle soit traitée par le gestionnaire d'erreurs global.
            }
        }




    }
}
