using ToursAndTravelsManagement.Models;
using Route = ToursAndTravelsManagement.Models.Route;

namespace ToursAndTravelsManagement.ViewModels
{
    public class HomePageViewModel
    {
        //public CabSearchViewModel? SearchModel { get; set; }
        //public List<City>? PopularCities { get; set; }
        //public List<Cab>?
        //{ get; set; }
        public List<City>? PopularCities { get; set; }
        public List<Cab>? FeaturedCabs { get; set; }
        public List<Route>? PopularRoutes { get; set; }
        public int TotalCabs { get; set; }
        public int TotalBookings { get; set; }
        public CabSearchViewModel? SearchModel { get; set; }


    }
}
