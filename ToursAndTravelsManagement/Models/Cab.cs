using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.Models
{
    public class Cab
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CabId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } // e.g., "SWIFT DZIRE", "TOYOTA INNOVA"

        [Required]
        public CabType Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [StringLength(20)]
        public string RegistrationNumber { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public bool HasAC { get; set; }

        [Required]
        public int SeatingCapacity { get; set; }

        [Required]
        public decimal BasePricePerKM { get; set; }

        public string? Features { get; set; } // JSON string of features
        public string? ImageUrl { get; set; }

        [Required]
        public CabStatus Status { get; set; }

        public string? DriverId { get; set; } // If you have drivers table
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<CabRoutePrice>? CabRoutePrices { get; set; }
    }
}

