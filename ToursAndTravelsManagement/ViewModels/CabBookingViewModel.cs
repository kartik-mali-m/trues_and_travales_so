using System.ComponentModel.DataAnnotations;
using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.ViewModels
{
    public class CabBookingViewModel
    {
        [Required]
        public int CabRoutePriceId { get; set; }

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
        [Range(1, 10)]
        public int NumberOfPassengers { get; set; }

        public string? AdditionalNotes { get; set; }

        // Guest booking details (if not logged in)
        [StringLength(100)]
        public string? GuestName { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? GuestEmail { get; set; }

        [StringLength(15)]
        [Phone]
        public string? GuestPhone { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }
    }
}
