using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.Models
{
    public class CabRoutePrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CabRoutePriceId { get; set; }

        [Required]
        public int CabId { get; set; }

        [ForeignKey("CabId")]
        public Cab? Cab { get; set; }

        [Required]
        public int RouteId { get; set; }

        [ForeignKey("RouteId")]
        public Route? Route { get; set; }

        [Required]
        public JourneyType JourneyType { get; set; }

        [Required]
        public decimal Price { get; set; }

        // Included services as JSON array
        public string? IncludedServices { get; set; }

        // Excluded services as JSON array
        public string? ExcludedServices { get; set; }

        public string? ExtraCharges { get; set; }
        public string? Notes { get; set; }
        public string? TermsAndConditions { get; set; }

        [Required]
        public bool IsAvailable { get; set; } = true;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
