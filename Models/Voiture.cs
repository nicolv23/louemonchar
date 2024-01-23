using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Projet_Final.Models
{
    public class Voiture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Marque { get; set; }
        public string Modèle { get; set; }
        public int Année { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public decimal PrixJournalier { get; set; }
        public bool EstDisponible { get; set; }
        [StringLength(255)]
        public string ImageVoiture { get; set; }
    }
}
