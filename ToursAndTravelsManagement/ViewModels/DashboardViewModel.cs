namespace ToursAndTravelsManagement.ViewModels
{
    public class DashboardViewModel
    {
        public Dictionary<string, int> BookingStatusData { get; set; }
        public Dictionary<string, decimal> RevenueByMonth { get; set; } // Assuming you want to show revenue by month from bookings
    }
}
