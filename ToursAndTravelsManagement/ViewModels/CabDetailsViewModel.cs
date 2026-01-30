using ToursAndTravelsManagement.Enums;

namespace ToursAndTravelsManagement.ViewModels
{
    public class CabDetailsViewModel
    {
        public int CabRoutePriceId { get; set; }
        public string CabName { get; set; }
        public CabType CabType { get; set; }
        public bool HasAC { get; set; }
        public int SeatingCapacity { get; set; }
        public decimal Price { get; set; }
        public string? FromCity { get; set; }
        public string? ToCity { get; set; }
        public JourneyType JourneyType { get; set; }
        public List<string> IncludedServices { get; set; } = new();
        public List<string> ExcludedServices { get; set; } = new();
        public string? ExtraCharges { get; set; }
        public string? Notes { get; set; }
        public string? Features { get; set; }
        public string? ImageUrl { get; set; }
    }
}
