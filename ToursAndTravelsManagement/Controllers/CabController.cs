using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.ViewModels;

namespace ToursAndTravelsManagement.Controllers
{
    //[Authorize]
    public class CabController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CabController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Cab/Search
        [AllowAnonymous]
        public IActionResult Search()
        {
            var popularCities = _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .ToList();

            ViewBag.PopularCities = popularCities;
            return View();
        }


        [AllowAnonymous]
        public IActionResult Index()
        {
            // Get popular cities
            var popularCities = _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .ToList();

            // Get featured cabs (for example, 6 cabs that are available and marked as featured)
            // Note: The Cab model does not have a Featured property. We can either add it or use some logic (like top 6 by price or rating).
            // Since there's no Featured property, let's just take the first 6 available cabs.
            var featuredCabs = _context.Cabs
                .Where(c => c.IsActive && c.Status == CabStatus.Available)
                .Take(6)
                .ToList();

            var viewModel = new HomePageViewModel
            {
                PopularCities = popularCities,
                FeaturedCabs = featuredCabs
            };

            return View(viewModel);
        }



        // Add to CabController.cs
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPopularCitiesPartial()
        {
            var cities = await _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .OrderBy(c => c.Name)
                .Take(8)
                .ToListAsync();

            return PartialView("_PopularCities", cities);
        }

        //[AllowAnonymous]
        //[HttpGet]
        //public async Task<IActionResult> GetFeaturedCabsPartial()
        //{
        //    var cabs = await _context.Cabs
        //        .Where(c => c.IsActive && c.Status == CabStatus.Available)
        //        .OrderByDescending(c => c.SeatingCapacity)
        //        .Take(4)
        //        .ToListAsync();

        //    return PartialView("_FeaturedCabs", cabs);
        //}
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetFeaturedCabsPartial()
        {
            // Get cabs that have at least one available CabRoutePrice
            var cabs = await _context.Cabs
                .Include(c => c.CabRoutePrices) // Include CabRoutePrices
                .Where(c => c.IsActive &&
                           c.Status == CabStatus.Available &&
                           c.CabRoutePrices.Any(crp => crp.IsAvailable && crp.IsActive))
                .OrderByDescending(c => c.SeatingCapacity)
                .Take(4)
                .ToListAsync();

            return PartialView("_FeaturedCabs", cabs);
        }

        // POST: Cab/SearchResults
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SearchResults(CabSearchViewModel model)
        {
            // Find route
            var route = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .FirstOrDefaultAsync(r =>
                    r.FromCity.Name == model.FromCity &&
                    r.ToCity.Name == model.ToCity &&
                    r.IsActive);

            if (route == null)
            {
                TempData["Error"] = "No route found between selected cities";
                return RedirectToAction("Search");
            }

            // Get available cab prices for this route and journey type
            var availableCabs = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                .ThenInclude(r => r.ToCity)
               .Where(crp =>
    crp.RouteId == route.RouteId &&
    crp.JourneyType == model.JourneyType &&
    crp.IsAvailable &&
    crp.IsActive &&
    crp.Cab != null &&
    crp.Cab.Status == CabStatus.Available &&
    crp.Cab.IsActive &&
    crp.Route != null) 
                .ToListAsync();

            ViewBag.SearchCriteria = model;
            return View(availableCabs);
        }

        // GET: Cab/CabDetails/{id}
        //[AllowAnonymous]
        //public async Task<IActionResult> CabDetails(int id)
        //{
        //    var cabRoutePrice = await _context.CabRoutePrices
        //        .Include(crp => crp.Cab)
        //        .Include(crp => crp.Route)
        //            .ThenInclude(r => r.FromCity)
        //        .Include(crp => crp.Route)
        //            .ThenInclude(r => r.ToCity)
        //        .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == id);

        //    if (cabRoutePrice == null)
        //    {
        //        return NotFound();
        //    }

        //    // Get similar cabs for recommendation
        //    var similarCabs = await _context.CabRoutePrices
        //        .Include(crp => crp.Cab)
        //        .Include(crp => crp.Route)
        //        .Where(crp =>
        //            crp.RouteId == cabRoutePrice.RouteId &&
        //            crp.CabRoutePriceId != id &&
        //            crp.IsAvailable &&
        //            crp.IsActive &&
        //            crp.Cab.Status == CabStatus.Available)
        //        .Take(3)
        //        .ToListAsync();

        //    ViewBag.SimilarCabs = similarCabs;
        //    return View(cabRoutePrice);
        //}

        [AllowAnonymous]
        public async Task<IActionResult> CabInfo(int id) // id is CabId
        {
            var cab = await _context.Cabs
                .FirstOrDefaultAsync(c => c.CabId == id && c.IsActive);

            if (cab == null)
            {
                return NotFound();
            }

            // We need to get popular cities for the search form dropdowns
            var popularCities = await _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .ToListAsync();

            ViewBag.PopularCities = popularCities;

            // Pass the CabId to the view so we can put it in a hidden field in the search form
            ViewBag.CabId = cab.CabId;

            return View(cab);
        }











        // GET: Cab/CabDetails/{id}
        [AllowAnonymous]
        public async Task<IActionResult> CabDetails(int id)
        {
            var cabRoutePrice = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == id);

            if (cabRoutePrice == null)
            {
                return NotFound();
            }

            // Get similar cabs for recommendation
            var similarCabs = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                .Where(crp =>
                    crp.RouteId == cabRoutePrice.RouteId &&
                    crp.CabRoutePriceId != id &&
                    crp.IsAvailable &&
                    crp.IsActive &&
                    crp.Cab.Status == CabStatus.Available)
                .Take(3)
                .ToListAsync();

            ViewBag.SimilarCabs = similarCabs;
            return View(cabRoutePrice);
        }












        // GET: Cab/Book/{id}
        [HttpGet]
        public async Task<IActionResult> Book(int id)
        {
            var cabRoutePrice = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == id);

            if (cabRoutePrice == null)
            {
                return NotFound();
            }

            var viewModel = new CabBookingViewModel
            {
                CabRoutePriceId = id,
                TravelDate = DateTime.Now.AddDays(1)
            };

            ViewBag.CabDetails = cabRoutePrice;
            return View(viewModel);
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Book(CabBookingViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        var cabRoutePrice = await _context.CabRoutePrices
        //            .Include(crp => crp.Cab)
        //            .Include(crp => crp.Route)
        //            .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == model.CabRoutePriceId);

        //        if (cabRoutePrice == null)
        //        {
        //            return NotFound();
        //        }

        //        // Check if cab is already booked for the same time
        //        var existingBooking = await _context.CabBookings
        //            .AnyAsync(b =>
        //                b.CabId == cabRoutePrice.CabId &&
        //                b.TravelDate.Date == model.TravelDate.Date &&
        //                b.Status != CabBookingStatus.Cancelled &&
        //                b.IsActive);

        //        if (existingBooking)
        //        {
        //            TempData["Error"] = "This cab is already booked for the selected date";
        //            ViewBag.CabDetails = cabRoutePrice;
        //            return View(model);
        //        }

        //        // ... rest of the booking logic
        //    }

        //    return View(model);
        //}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(CabBookingViewModel model)
        {
            var cabRoutePrice = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == model.CabRoutePriceId);

            if (cabRoutePrice == null)
                return NotFound();

            ViewBag.CabDetails = cabRoutePrice; // ⭐ VERY IMPORTANT

            if (!ModelState.IsValid)
                return View(model);

            var existingBooking = await _context.CabBookings.AnyAsync(b =>
                b.CabId == cabRoutePrice.CabId &&
                b.TravelDate.Date == model.TravelDate.Date &&
                b.Status != CabBookingStatus.Cancelled &&
                b.IsActive);

            if (existingBooking)
            {
                TempData["Error"] = "This cab is already booked for the selected date";
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);

            var booking = new CabBooking
            {
                CabRoutePriceId = model.CabRoutePriceId,
                CabId = cabRoutePrice.CabId,
                TravelDate = model.TravelDate,
                PickupTime = model.PickupTime,
                PickupLocation = model.PickupLocation,
                DropoffLocation = model.DropoffLocation,
                NumberOfPassengers = model.NumberOfPassengers,
                AdditionalNotes = model.AdditionalNotes,
                BasePrice = cabRoutePrice.Price,
                TotalPrice = cabRoutePrice.Price,
                Status = CabBookingStatus.Pending,
                PaymentMethod = model.PaymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            if (user != null)
            {
                booking.UserId = user.Id;
                booking.CreatedBy = user.UserName;
            }
            else
            {
                booking.GuestName = model.GuestName;
                booking.GuestEmail = model.GuestEmail;
                booking.GuestPhone = model.GuestPhone;
                booking.CreatedBy = "Guest";
            }

            _context.CabBookings.Add(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction("BookingConfirmation", new { id = booking.CabBookingId });
        }



        //  [HttpPost]
        //  [ValidateAntiForgeryToken]
        //   public async Task<IActionResult> Book(CabBookingViewModel model)
        //   {
        //       if (ModelState.IsValid)
        //       {
        //           var user = await _userManager.GetUserAsync(User);
        //           var cabRoutePrice = await _context.CabRoutePrices
        //               .Include(crp => crp.Cab)
        //               .Include(crp => crp.Route)
        //               .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == model.CabRoutePriceId);

        //           if (cabRoutePrice == null)
        //           {
        //               return NotFound();
        //           }


        //           // check if cab is already booked for the same time

        //           var existingBooking = await _context.CabBookings
        //.AnyAsync(b =>
        //    b.CabId == cabRoutePrice.CabId &&
        //    b.TravelDate.Date == model.TravelDate.Date &&
        //    b.Status != CabBookingStatus.Cancelled &&
        //    b.IsActive);

        //           if (existingBooking)
        //           {
        //               TempData["Error"] = "This cab is already booked for the selected date";
        //               ViewBag.CabDetails = cabRoutePrice;
        //               return View(model);
        //           }



        //           var booking = new CabBooking
        //           {
        //               CabRoutePriceId = model.CabRoutePriceId,
        //               CabId = cabRoutePrice.CabId,
        //               TravelDate = model.TravelDate,
        //               PickupTime = model.PickupTime,
        //               PickupLocation = model.PickupLocation,
        //               DropoffLocation = model.DropoffLocation,
        //               NumberOfPassengers = model.NumberOfPassengers,
        //               AdditionalNotes = model.AdditionalNotes,
        //               BasePrice = cabRoutePrice.Price,
        //               TotalPrice = cabRoutePrice.Price, // Add extra charges logic here
        //               Status = CabBookingStatus.Pending,
        //               PaymentMethod = model.PaymentMethod,
        //               PaymentStatus = PaymentStatus.Pending,
        //               CreatedDate = DateTime.Now,
        //               IsActive = true
        //           };

        //           if (user != null)
        //           {
        //               booking.UserId = user.Id;
        //               booking.CreatedBy = user.UserName;
        //           }
        //           else
        //           {
        //               booking.GuestName = model.GuestName;
        //               booking.GuestEmail = model.GuestEmail;
        //               booking.GuestPhone = model.GuestPhone;
        //               booking.CreatedBy = "Guest";
        //           }

        //           _context.CabBookings.Add(booking);
        //           await _context.SaveChangesAsync();

        //           // Update cab status if needed
        //           var cab = await _context.Cabs.FindAsync(cabRoutePrice.CabId);
        //           if (cab != null)
        //           {
        //               cab.Status = CabStatus.Booked;
        //               _context.Cabs.Update(cab);
        //           }

        //           await _context.SaveChangesAsync();

        //           return RedirectToAction("BookingConfirmation", new { id = booking.CabBookingId });
        //       }

        //       return View(model);
        //   }

        // GET: Cab/BookingConfirmation/{id}
        public async Task<IActionResult> BookingConfirmation(int id)
        {
            var booking = await _context.CabBookings
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Cab)
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Route)
                .ThenInclude(r => r.FromCity)
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Route)
                .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(b => b.CabBookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Cab/MyBookings
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookings = await _context.CabBookings
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Cab)
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Route)
                .ThenInclude(r => r.FromCity)
                .Include(b => b.CabRoutePrice)
                .ThenInclude(crp => crp.Route)
                .ThenInclude(r => r.ToCity)
                .Where(b => b.UserId == user.Id)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return View(bookings);
        }


        // Add these methods to your existing CabController

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.CabBookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Check if user owns this booking
            var user = await _userManager.GetUserAsync(User);
            if (booking.UserId != user?.Id)
            {
                return Forbid();
            }

            // Only allow cancellation for pending/confirmed bookings
            if (booking.Status != CabBookingStatus.Pending && booking.Status != CabBookingStatus.Confirmed)
            {
                TempData["Error"] = "Cannot cancel booking in current status";
                return RedirectToAction("MyBookings");
            }

            booking.Status = CabBookingStatus.Cancelled;
            booking.UpdatedDate = DateTime.Now;
            booking.UpdatedBy = user?.UserName;

            // Make cab available again
            var cab = await _context.Cabs.FindAsync(booking.CabId);
            if (cab != null)
            {
                cab.Status = CabStatus.Available;
                _context.Cabs.Update(cab);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking cancelled successfully";
            return RedirectToAction("MyBookings");
        }

        public async Task<IActionResult> DownloadInvoice(int id)
        {
            var booking = await _context.CabBookings
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Cab)
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.CabBookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            // Generate PDF invoice (you'll need to implement PDF generation)
            // For now, return a view
            return View("Invoice", booking);
        }

        public async Task<IActionResult> GetAvailableCities()
        {
            var cities = await _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .OrderBy(c => c.Name)
                .Select(c => new { c.CityId, c.Name })
                .ToListAsync();

            return Json(cities);
        }





        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Book(CabBookingViewModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var user = await _userManager.GetUserAsync(User);
        //        var cabRoutePrice = await _context.CabRoutePrices
        //            .Include(crp => crp.Cab)
        //            .Include(crp => crp.Route)
        //            .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == model.CabRoutePriceId);

        //        if (cabRoutePrice == null)
        //        {
        //            return NotFound();
        //        }

        //        // Check if cab is already booked for the same time
        //        var existingBooking = await _context.CabBookings
        //            .AnyAsync(b =>
        //                b.CabId == cabRoutePrice.CabId &&
        //                b.TravelDate.Date == model.TravelDate.Date &&
        //                b.Status != CabBookingStatus.Cancelled &&
        //                b.IsActive);

        //        if (existingBooking)
        //        {
        //            TempData["Error"] = "This cab is already booked for the selected date";
        //            ViewBag.CabDetails = cabRoutePrice;
        //            return View(model);
        //        }

        //        // ... rest of the booking logic
        //    }

        //    return View(model);
        //}

    }
}
