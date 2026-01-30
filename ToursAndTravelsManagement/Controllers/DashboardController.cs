using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.ViewModels;


[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DashboardController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: Dashboard
    //public async Task<IActionResult> Index()
    //{
    //    var bookings = await _unitOfWork.BookingRepository.GetAllAsync();

    //    var model = new DashboardViewModel
    //    {
    //        BookingStatusData = bookings.GroupBy(b => b.Status.ToString())
    //                                    .ToDictionary(g => g.Key, g => g.Count()),

    //        RevenueByMonth = bookings.GroupBy(b => b.BookingDate.ToString("MMMM yyyy"))
    //                                 .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice))
    //    };

    //    return View(model);
    //}




     // GET: Dashboard
    [Authorize(Roles = "Admin")]  // This can also be added at action level
    public async Task<IActionResult> Index()
    {
        var bookings = await _unitOfWork.BookingRepository.GetAllAsync();

        var model = new DashboardViewModel
        {
            BookingStatusData = bookings.GroupBy(b => b.Status.ToString())
                                        .ToDictionary(g => g.Key, g => g.Count()),

            RevenueByMonth = bookings.GroupBy(b => b.BookingDate.ToString("MMMM yyyy"))
                                     .ToDictionary(g => g.Key, g => g.Sum(b => b.TotalPrice))
        };

        return View(model);
    }
}

