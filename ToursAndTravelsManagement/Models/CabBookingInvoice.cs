using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ToursAndTravelsManagement.Models
{
    public class CabBookingInvoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int InvoiceId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = $"INV-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4)}";

        [Required]
        public int CabBookingId { get; set; }

        [ForeignKey("CabBookingId")]
        public CabBooking? CabBooking { get; set; }

        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);

        public string? BillingAddress { get; set; }

        // Pricing breakdown
        [Required]
        public decimal BaseFare { get; set; }

        public decimal TollCharges { get; set; }
        public decimal DriverAllowance { get; set; }
        public decimal ParkingCharges { get; set; }
        public decimal NightCharges { get; set; }
        public decimal GST { get; set; }
        public decimal OtherCharges { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }

        public string? PaymentDetails { get; set; }
        public string? Notes { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

