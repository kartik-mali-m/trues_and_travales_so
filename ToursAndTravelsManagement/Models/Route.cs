using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Models
{
    public class Route
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RouteId { get; set; }

        [Required]
        public int FromCityId { get; set; }

        [ForeignKey("FromCityId")]
        public City? FromCity { get; set; }

        [Required]
        public int ToCityId { get; set; }

        [ForeignKey("ToCityId")]
        public City? ToCity { get; set; }

        [Required]
        public decimal Distance { get; set; } // in KM

        [Required]
        public decimal EstimatedTime { get; set; } // in hours

        public string? RouteDescription { get; set; }
        public string? PopularStops { get; set; } // JSON array

        [Required]
        public bool IsActive { get; set; } = true;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
