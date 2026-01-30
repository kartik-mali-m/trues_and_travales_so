using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.Models
{
    public class CabBooking
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CabBookingId { get; set; }

        [Required]
        public string BookingNumber { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // Guest booking details (if not logged in)
        [StringLength(100)]
        public string? GuestName { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? GuestEmail { get; set; }

        [StringLength(15)]
        public string? GuestPhone { get; set; }

        // Booking details
        [Required]
        public int CabRoutePriceId { get; set; }

        [ForeignKey("CabRoutePriceId")]
        public CabRoutePrice? CabRoutePrice { get; set; }

        [Required]
        public DateTime TravelDate { get; set; }

        [Required]
        public TimeSpan PickupTime { get; set; }

        [Required]
        [StringLength(500)]
        public string PickupLocation { get; set; }

        [Required]
        [StringLength(500)]
        public string DropoffLocation { get; set; }

        [Required]
        public int NumberOfPassengers { get; set; }

        public string? AdditionalNotes { get; set; }

        // Pricing
        [Required]
        public decimal BasePrice { get; set; }

        public decimal ExtraCharges { get; set; }
        public decimal Discount { get; set; }

        [Required]
        public decimal TotalPrice { get; set; }

        // Status
        [Required]
        public CabBookingStatus Status { get; set; } = CabBookingStatus.Pending;

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string? PaymentTransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Admin fields
        public string? AssignedDriverId { get; set; }
        public string? AssignedDriverName { get; set; }
        public string? AssignedDriverPhone { get; set; }
        public string? VehicleNumber { get; set; }

        // Timestamps
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public int? CabId { get; set; }
        [ForeignKey("CabId")]
        public Cab? Cab { get; set; }

        public CabBookingInvoice? CabBookingInvoice { get; set; }

    }
}
