namespace ToursAndTravelsManagement.ViewModels
{
    public class CabSearchViewModel
    {
        public string FromCity { get; set; }
        public string ToCity { get; set; }
        public DateTime TravelDate { get; set; }
        public Enums.JourneyType JourneyType { get; set; }
    }
}
