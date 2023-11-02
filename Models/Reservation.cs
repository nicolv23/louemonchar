using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projet_Final.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int VoitureId { get; set; }
        public Voiture Voiture { get; set; }

        public int ClientId { get; set; }
        public Client Client { get; set; }

        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
    }
}
