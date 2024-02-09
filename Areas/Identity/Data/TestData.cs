using System;
using System.Collections.Generic;

namespace Projet_Final.Models
{
    public class TestData
    {
        public List<Voiture> GetVoitures()
        {
            return new List<Voiture>
            {
                new Voiture
                {
                    Marque = "Toyota",
                    Modèle = "Corolla",
                    Année = 2022,
                    PrixJournalier = 50.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/toyota_corolla.jpg"
                },
                new Voiture
                {
                    Marque = "Honda",
                    Modèle = "Civic",
                    Année = 2023,
                    PrixJournalier = 55.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/honda_civic.jpg"
                },
                new Voiture
                {
                    Marque = "Ford",
                    Modèle = "Mustang",
                    Année = 2022,
                    PrixJournalier = 70.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/ford_mustang.jpg"
                },
                new Voiture
                {
                    Marque = "Chevrolet",
                    Modèle = "Camaro",
                    Année = 2023,
                    PrixJournalier = 75.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/chevrolet_camaro.jpeg"
                },
                new Voiture
                {
                    Marque = "Tesla",
                    Modèle = "Model S",
                    Année = 2023,
                    PrixJournalier = 100.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/tesla_modele_s.jpeg"
                },
                new Voiture
                {
                    Marque = "BMW",
                    Modèle = "M3",
                    Année = 2022,
                    PrixJournalier = 80.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/bmw_m3.jpeg"
                },
                new Voiture
                {
                    Marque = "Audi",
                    Modèle = "A4",
                    Année = 2023,
                    PrixJournalier = 65.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/audi_a4.jpg"
                },
                new Voiture
                {
                    Marque = "Mercedes",
                    Modèle = "C-Class",
                    Année = 2022,
                    PrixJournalier = 90.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/mercedes_classe_c.jpeg"
                },
                new Voiture
                {
                    Marque = "Lamborghini",
                    Modèle = "Huracan",
                    Année = 2023,
                    PrixJournalier = 500.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/lamborghini_huracan.jpg"
                },
                new Voiture
                {
                    Marque = "Ferrari",
                    Modèle = "488 GTB",
                    Année = 2023,
                    PrixJournalier = 600.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/ferrari_488gtb.jpg"
                },
                new Voiture
                {
                    Marque = "Nissan",
                    Modèle = "GT-R",
                    Année = 2020,
                    PrixJournalier = 205.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/nissan_gtr.jpg"
                },
                new Voiture
                {
                    Marque = "Nissan",
                    Modèle = "Sentra",
                    Année = 2024,
                    PrixJournalier = 120.00m,
                    EstDisponible = true,
                    ImageVoiture = "~/images/voitures/nissan_sentra.jpg"
                }
            };
        }

        public List<Reservation> GetReservations(List<Voiture> voitures, List<Client> clients)
        {
            var reservations = new List<Reservation>();

           
            for (int i = 0; i < Math.Min(voitures.Count, clients.Count); i++)
            {
                var reservation = new Reservation
                {
                    VoitureId = voitures[i].Id,
                    Voiture = voitures[i], 
                    ClientId = clients[i].ClientId,
                    Client = clients[i], 
                    DateDebut = new DateTime(2023, 1, 1).AddDays(i),
                    DateFin = new DateTime(2023, 1, 7).AddDays(i) 
                };

                reservations.Add(reservation);
            }

            return reservations;
        }


        public List<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    Nom = "Dupont",
                    Prénom = "Alice",
                    Adresse = "123 Rue de la République",
                    NuméroTéléphone = "0123456789"
                },
                new Client
                {
                    Nom = "Martin",
                    Prénom = "Jean",
                    Adresse = "456 Avenue des Fleurs",
                    NuméroTéléphone = "9876543210"
                },
                new Client
                {
                    Nom = "Lefebvre",
                    Prénom = "Sophie",
                    Adresse = "789 Rue de la Paix",
                    NuméroTéléphone = "1112233445"
                },
                new Client
                {
                    Nom = "Leclerc",
                    Prénom = "Pierre",
                    Adresse = "1010 Boulevard Voltaire",
                    NuméroTéléphone = "5432167890"
                },
                new Client
                {
                    Nom = "Dufresne",
                    Prénom = "Marie",
                    Adresse = "1212 Avenue Montaigne",
                    NuméroTéléphone = "6789054321"
                },
                new Client
                {
                    Nom = "Girard",
                    Prénom = "Luc",
                    Adresse = "1414 Rue de la Liberté",
                    NuméroTéléphone = "1122334455"
                },
                new Client
                {
                    Nom = "Bergeron",
                    Prénom = "Isabelle",
                    Adresse = "1616 Avenue des Champs-Élysées",
                    NuméroTéléphone = "9988776655"
                },
                new Client
                {
                    Nom = "Thomas",
                    Prénom = "Paul",
                    Adresse = "1818 Rue de la Concorde",
                    NuméroTéléphone = "2233445566"
                },
                new Client
                {
                    Nom = "Roussel",
                    Prénom = "Laura",
                    Adresse = "2020 Boulevard Haussmann",
                    NuméroTéléphone = "3344556677"
                },
                new Client
                {
                    Nom = "Morin",
                    Prénom = "Olivier",
                    Adresse = "2222 Avenue Victor Hugo",
                    NuméroTéléphone = "4455667788"
                }
            };
        }
    }
}
