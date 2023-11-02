using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projet_Final.Models
{
    public class Client
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClientId { get; set; }
        public string Nom { get; set; }
        public string Prénom { get; set; }
        public string Adresse { get; set; }
        public string NuméroTéléphone { get; set; }
    }
}
