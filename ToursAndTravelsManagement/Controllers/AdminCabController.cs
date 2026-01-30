using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.ViewModels;
using Route = ToursAndTravelsManagement.Models.Route;

namespace ToursAndTravelsManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCabController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminCabController> _logger;

        public AdminCabController(ApplicationDbContext context, ILogger<AdminCabController> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Dashboard
        public async Task<IActionResult> Dashboard()
        {
            ViewData["Title"] = "Dashboard";
            var dashboard = new AdminCabDashboardViewModel
            {
                TotalCabs = await _context.Cabs.CountAsync(),
                AvailableCabs = await _context.Cabs.CountAsync(c => c.Status == CabStatus.Available),
                TotalBookings = await _context.CabBookings.CountAsync(),
                TodayBookings = await _context.CabBookings
                    .Where(b => b.CreatedDate.Date == DateTime.Today)
                    .CountAsync(),
                PendingBookings = await _context.CabBookings
                    .Where(b => b.Status == CabBookingStatus.Pending)
                    .CountAsync(),
                TotalRevenue = await _context.CabBookings
                    .Where(b => b.Status == CabBookingStatus.Completed)
                    .SumAsync(b => b.TotalPrice),
                RecentBookings = await _context.CabBookings
                    .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                    .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                    .OrderByDescending(b => b.CreatedDate)
                    .Take(10)
                    .ToListAsync()
            };

            return View(dashboard);
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> GetPendingBookingsCount()
        {
            ViewData["Title"] = "Manage Cabs";
            var count = await _context.CabBookings
                .Where(b => b.Status == CabBookingStatus.Pending && b.IsActive)
                .CountAsync();

            return Json(count);
        }




        //Manage Cabs
        #region CRUD for Cabs
        [HttpGet]
        public async Task<IActionResult> ManageCabs(string status = "")
        {
            var query = _context.Cabs.Where(c => c.IsActive);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<CabStatus>(status, out var cabStatus))
                {
                    query = query.Where(c => c.Status == cabStatus);
                }
            }

            var cabs = await query
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Status)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            return View(cabs);
        }

        [HttpGet]
        public IActionResult CreateCab()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCab(Cab cab)
        {
            if (ModelState.IsValid)
            {
                // Check if registration number already exists
                var existingCab = await _context.Cabs
                    .FirstOrDefaultAsync(c => c.RegistrationNumber == cab.RegistrationNumber && c.IsActive);

                if (existingCab != null)
                {
                    ModelState.AddModelError("RegistrationNumber", "A cab with this registration number already exists.");
                    return View(cab);
                }

                cab.CreatedDate = DateTime.Now;
                cab.CreatedBy = User.Identity.Name;
                cab.IsActive = true;

                _context.Cabs.Add(cab);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab added successfully!";
                return RedirectToAction("ManageCabs");
            }
            return View(cab);
        }

        [HttpGet]
        public async Task<IActionResult> EditCab(int id)
        {
            var cab = await _context.Cabs.FindAsync(id);
            if (cab == null)
            {
                return NotFound();
            }
            return View(cab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCab(int id, Cab updatedCab)
        {
            if (id != updatedCab.CabId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cab = await _context.Cabs.FindAsync(id);
                if (cab == null)
                {
                    return NotFound();
                }

                // Check if registration number already exists for another cab
                var existingCab = await _context.Cabs
                    .FirstOrDefaultAsync(c => c.RegistrationNumber == updatedCab.RegistrationNumber
                        && c.CabId != id
                        && c.IsActive);

                if (existingCab != null)
                {
                    ModelState.AddModelError("RegistrationNumber", "A cab with this registration number already exists.");
                    return View(updatedCab);
                }

                cab.Name = updatedCab.Name;
                cab.Type = updatedCab.Type;
                cab.Model = updatedCab.Model;
                cab.RegistrationNumber = updatedCab.RegistrationNumber;
                cab.Year = updatedCab.Year;
                cab.HasAC = updatedCab.HasAC;
                cab.SeatingCapacity = updatedCab.SeatingCapacity;
                cab.BasePricePerKM = updatedCab.BasePricePerKM;
                cab.Features = updatedCab.Features;
                cab.ImageUrl = updatedCab.ImageUrl;
                cab.Status = updatedCab.Status;
                cab.DriverName = updatedCab.DriverName;
                cab.DriverPhone = updatedCab.DriverPhone;

                _context.Cabs.Update(cab);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab updated successfully!";
                return RedirectToAction("ManageCabs");
            }
            return View(updatedCab);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCab(int id)
        {
            var cab = await _context.Cabs.FindAsync(id);
            if (cab != null)
            {
                cab.IsActive = false;
                _context.Cabs.Update(cab);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab deleted successfully!";
            }
            return RedirectToAction("ManageCabs");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCabStatus(int id)
        {
            var cab = await _context.Cabs.FindAsync(id);
            if (cab != null)
            {
                cab.Status = cab.Status == CabStatus.Available
                    ? CabStatus.Unavailable
                    : CabStatus.Available;

                _context.Cabs.Update(cab);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cab status updated to {cab.Status} successfully!";
            }
            return RedirectToAction("ManageCabs");
        }
        #endregion



        #region CRUD for Cities
        public async Task<IActionResult> ManageCities()
        {
            var cities = await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(cities);
        }

        [HttpGet]
        public IActionResult CreateCity()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCity(City city)
        {
            if (ModelState.IsValid)
            {
                city.CreatedDate = DateTime.Now;
                city.CreatedBy = User.Identity.Name;
                city.IsActive = true;

                _context.Cities.Add(city);
                await _context.SaveChangesAsync();

                TempData["Success"] = "City added successfully!";
                return RedirectToAction("ManageCities");
            }
            return View(city);
        }

        [HttpGet]
        public async Task<IActionResult> EditCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, City updatedCity)
        {
            if (id != updatedCity.CityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var city = await _context.Cities.FindAsync(id);
                if (city == null)
                {
                    return NotFound();
                }

                city.Name = updatedCity.Name;
                city.State = updatedCity.State;
                city.Country = updatedCity.Country;
                city.Description = updatedCity.Description;
                city.ImageUrl = updatedCity.ImageUrl;
                city.IsPopular = updatedCity.IsPopular;

                _context.Cities.Update(city);
                await _context.SaveChangesAsync();

                TempData["Success"] = "City updated successfully!";
                return RedirectToAction("ManageCities");
            }
            return View(updatedCity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCity(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                city.IsActive = false;
                _context.Cities.Update(city);
                await _context.SaveChangesAsync();

                TempData["Success"] = "City deleted successfully!";
            }
            return RedirectToAction("ManageCities");
        }
        #endregion

        #region CRUD for Routes
        public async Task<IActionResult> ManageRoutes()
        {
            var routes = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCity.Name)
                .ThenBy(r => r.ToCity.Name)
                .ToListAsync();
            return View(routes);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRoute()
        {
            var cities = await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Cities = cities;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRoute(Route route)
        {
            if (route.FromCityId == route.ToCityId)
            {
                ModelState.AddModelError("", "From and To cities cannot be the same");
            }

            if (ModelState.IsValid)
            {
                // Check if route already exists
                var existingRoute = await _context.Routes
                    .FirstOrDefaultAsync(r =>
                        r.FromCityId == route.FromCityId &&
                        r.ToCityId == route.ToCityId &&
                        r.IsActive);

                if (existingRoute != null)
                {
                    TempData["Error"] = "Route already exists between these cities";
                    var cities = await _context.Cities.Where(c => c.IsActive).ToListAsync();
                    ViewBag.Cities = cities;
                    return View(route);
                }

                route.CreatedDate = DateTime.Now;
                route.CreatedBy = User.Identity.Name;
                route.IsActive = true;

                _context.Routes.Add(route);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Route added successfully!";
                return RedirectToAction("ManageRoutes");
            }

            var allCities = await _context.Cities.Where(c => c.IsActive).ToListAsync();
            ViewBag.Cities = allCities;
            return View(route);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoute(int id)
        {
            var route = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .FirstOrDefaultAsync(r => r.RouteId == id);

            if (route == null)
            {
                return NotFound();
            }

            var cities = await _context.Cities
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.Cities = cities;

            return View(route);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoute(int id, Route updatedRoute)
        {
            if (id != updatedRoute.RouteId)
            {
                return NotFound();
            }

            if (updatedRoute.FromCityId == updatedRoute.ToCityId)
            {
                ModelState.AddModelError("", "From and To cities cannot be the same");
            }

            if (ModelState.IsValid)
            {
                var route = await _context.Routes.FindAsync(id);
                if (route == null)
                {
                    return NotFound();
                }

                route.FromCityId = updatedRoute.FromCityId;
                route.ToCityId = updatedRoute.ToCityId;
                route.Distance = updatedRoute.Distance;
                route.EstimatedTime = updatedRoute.EstimatedTime;
                route.RouteDescription = updatedRoute.RouteDescription;
                route.PopularStops = updatedRoute.PopularStops;

                _context.Routes.Update(route);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Route updated successfully!";
                return RedirectToAction("ManageRoutes");
            }

            var cities = await _context.Cities.Where(c => c.IsActive).ToListAsync();
            ViewBag.Cities = cities;
            return View(updatedRoute);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                route.IsActive = false;
                _context.Routes.Update(route);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Route deleted successfully!";
            }
            return RedirectToAction("ManageRoutes");
        }
        #endregion

        #region CRUD for CabRoutePrices
        //public async Task<IActionResult> ManageCabPrices()
        //{
        //    var cabPrices = await _context.CabRoutePrices
        //        .Include(crp => crp.Cab)
        //        .Include(crp => crp.Route)
        //            .ThenInclude(r => r.FromCity)
        //        .Include(crp => crp.Route)
        //            .ThenInclude(r => r.ToCity)
        //        .Where(crp => crp.IsActive)
        //        .OrderBy(crp => crp.Route.FromCity.Name)
        //        .ThenBy(crp => crp.Route.ToCity.Name)
        //        .ToListAsync();
        //    return View(cabPrices);
        //}

        public async Task<IActionResult> ManageCabPrices()
        {
            var cabPrices = await _context.CabRoutePrices
                .AsNoTracking()                             // performance + read-only
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r!.FromCity)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r!.ToCity)
                .Where(crp => crp.IsActive)
                .Where(crp => crp.Cab!.IsActive)            // usually wanted
                .Where(crp => crp.Route!.IsActive)
                .Where(crp => crp.Route!.FromCity!.IsActive && crp.Route!.ToCity!.IsActive)
                .OrderBy(crp => crp.Route!.FromCity!.Name)
                .ThenBy(crp => crp.Route!.ToCity!.Name)
                .ToListAsync();

            return View(cabPrices);
        }


        [HttpGet]
        public async Task<IActionResult> CabPriceDetails(int id)
        {
            var cabPrice = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == id);

            if (cabPrice == null)
            {
                TempData["Error"] = "Cab price configuration not found.";
                return RedirectToAction(nameof(ManageCabPrices));
            }

            return View(cabPrice);
        }








        [HttpGet]
        public async Task<IActionResult> CreateCabPrice()
        {
            var cabs = await _context.Cabs
                .Where(c => c.IsActive && c.Status == CabStatus.Available)
                .OrderBy(c => c.Name)
                .ToListAsync();
            var routes = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCity.Name)
                .ThenBy(r => r.ToCity.Name)
                .ToListAsync();

            ViewBag.Cabs = cabs;
            ViewBag.Routes = routes;
            ViewBag.JourneyTypes = Enum.GetValues(typeof(JourneyType))
                .Cast<JourneyType>()
                .Select(j => new { Id = (int)j, Name = j.ToString() })
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCabPrice(CabRoutePrice cabPrice)
        {
            if (ModelState.IsValid)
            {
                // Check if price already exists for this cab-route-journey combination
                var existingPrice = await _context.CabRoutePrices
                    .FirstOrDefaultAsync(crp =>
                        crp.CabId == cabPrice.CabId &&
                        crp.RouteId == cabPrice.RouteId &&
                        crp.JourneyType == cabPrice.JourneyType &&
                        crp.IsActive);

                if (existingPrice != null)
                {
                    TempData["Error"] = "Price already exists for this cab, route, and journey type combination";
                    await LoadCreateCabPriceViewData();
                    return View(cabPrice);
                }

                cabPrice.CreatedDate = DateTime.Now;
                cabPrice.CreatedBy = User.Identity.Name;
                cabPrice.IsActive = true;
                cabPrice.IsAvailable = true;

                _context.CabRoutePrices.Add(cabPrice);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab price added successfully!";
                return RedirectToAction("ManageCabPrices");
            }

            await LoadCreateCabPriceViewData();
            return View(cabPrice);
        }

        [HttpGet]
        public async Task<IActionResult> EditCabPrice(int id)
        {
            var cabPrice = await _context.CabRoutePrices
                .Include(crp => crp.Cab)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .FirstOrDefaultAsync(crp => crp.CabRoutePriceId == id);

            if (cabPrice == null)
            {
                return NotFound();
            }

            await LoadEditCabPriceViewData();
            return View(cabPrice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCabPrice(int id, CabRoutePrice updatedCabPrice)
        {
            if (id != updatedCabPrice.CabRoutePriceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var cabPrice = await _context.CabRoutePrices.FindAsync(id);
                if (cabPrice == null)
                {
                    return NotFound();
                }

                cabPrice.CabId = updatedCabPrice.CabId;
                cabPrice.RouteId = updatedCabPrice.RouteId;
                cabPrice.JourneyType = updatedCabPrice.JourneyType;
                cabPrice.Price = updatedCabPrice.Price;
                cabPrice.IncludedServices = updatedCabPrice.IncludedServices;
                cabPrice.ExcludedServices = updatedCabPrice.ExcludedServices;
                cabPrice.ExtraCharges = updatedCabPrice.ExtraCharges;
                cabPrice.Notes = updatedCabPrice.Notes;
                cabPrice.TermsAndConditions = updatedCabPrice.TermsAndConditions;
                cabPrice.IsAvailable = updatedCabPrice.IsAvailable;

                _context.CabRoutePrices.Update(cabPrice);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab price updated successfully!";
                return RedirectToAction("ManageCabPrices");
            }

            await LoadEditCabPriceViewData();
            return View(updatedCabPrice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCabPrice(int id)
        {
            var cabPrice = await _context.CabRoutePrices.FindAsync(id);
            if (cabPrice != null)
            {
                cabPrice.IsActive = false;
                _context.CabRoutePrices.Update(cabPrice);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cab price deleted successfully!";
            }
            return RedirectToAction("ManageCabPrices");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var cabPrice = await _context.CabRoutePrices.FindAsync(id);
            if (cabPrice != null)
            {
                cabPrice.IsAvailable = !cabPrice.IsAvailable;
                _context.CabRoutePrices.Update(cabPrice);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cab price {(cabPrice.IsAvailable ? "activated" : "deactivated")} successfully!";
            }
            return RedirectToAction("ManageCabPrices");
        }

        private async Task LoadCreateCabPriceViewData()
        {
            var cabs = await _context.Cabs
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            var routes = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCity.Name)
                .ThenBy(r => r.ToCity.Name)
                .ToListAsync();

            ViewBag.Cabs = cabs;
            ViewBag.Routes = routes;
            ViewBag.JourneyTypes = Enum.GetValues(typeof(JourneyType))
                .Cast<JourneyType>()
                .Select(j => new { Id = (int)j, Name = j.ToString() })
                .ToList();
        }

        private async Task LoadEditCabPriceViewData()
        {
            var cabs = await _context.Cabs
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
            var routes = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCity.Name)
                .ThenBy(r => r.ToCity.Name)
                .ToListAsync();

            ViewBag.Cabs = cabs;
            ViewBag.Routes = routes;
            ViewBag.JourneyTypes = Enum.GetValues(typeof(JourneyType))
                .Cast<JourneyType>()
                .Select(j => new { Id = (int)j, Name = j.ToString() })
                .ToList();
        }
        #endregion

        #region Manage Bookings
        //public async Task<IActionResult> ManageBookings(string status = "")
        //{
        //    var query = _context.CabBookings
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Cab)
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Route)
        //            .ThenInclude(r => r.FromCity)
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Route)
        //            .ThenInclude(r => r.ToCity)
        //        .Include(b => b.User)
        //        .Where(b => b.IsActive);

        //    if (!string.IsNullOrEmpty(status))
        //    {
        //        if (Enum.TryParse<CabBookingStatus>(status, out var bookingStatus))
        //        {
        //            query = query.Where(b => b.Status == bookingStatus);
        //        }
        //    }

        //    var bookings = await query
        //        .OrderByDescending(b => b.CreatedDate)
        //        .ToListAsync();

        //    ViewBag.StatusFilter = status;
        //    return View(bookings);
        //}

        public async Task<IActionResult> ManageBookings(
     string status = "",
     string search = "",
     DateTime? fromDate = null,
     DateTime? toDate = null)
        {

            ViewData["Title"] = "Manage Bookings";
            var query = _context.CabBookings
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Cab)
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.FromCity)
                .Include(b => b.CabRoutePrice)
                    .ThenInclude(crp => crp.Route)
                    .ThenInclude(r => r.ToCity)
                .Include(b => b.User)
                .Where(b => b.IsActive);

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<CabBookingStatus>(status, out var bookingStatus))
                {
                    query = query.Where(b => b.Status == bookingStatus);
                }
            }

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b =>
                    b.BookingNumber.Contains(search) ||
                    b.GuestName != null && b.GuestName.Contains(search) ||
                    b.GuestEmail != null && b.GuestEmail.Contains(search) ||
                    b.GuestPhone != null && b.GuestPhone.Contains(search) ||
                    b.AssignedDriverName != null && b.AssignedDriverName.Contains(search) ||
                    b.VehicleNumber != null && b.VehicleNumber.Contains(search) ||
                    (b.User != null && (b.User.UserName.Contains(search) ||
                                      b.User.Email.Contains(search)))
                );
            }

            // Date range filter
            if (fromDate.HasValue)
            {
                query = query.Where(b => b.CreatedDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(b => b.CreatedDate <= toDate.Value.AddDays(1));
            }

            var bookings = await query
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            ViewBag.StatusFilter = status;
            ViewBag.SearchString = search;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            return View(bookings);
        }



        //[HttpPost]
        //public async Task<IActionResult> ExportBookings(string format = "excel")
        //{
        //    var bookings = await _context.CabBookings
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Cab)
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Route)
        //            .ThenInclude(r => r.FromCity)
        //        .Include(b => b.CabRoutePrice)
        //            .ThenInclude(crp => crp.Route)
        //            .ThenInclude(r => r.ToCity)
        //        .Include(b => b.User)
        //        .Where(b => b.IsActive)
        //        .OrderByDescending(b => b.CreatedDate)
        //        .ToListAsync();

        //    if (format.ToLower() == "excel")
        //    {
        //        // Generate Excel file
        //        return GenerateExcel(bookings);
        //    }
        //    else if (format.ToLower() == "pdf")
        //    {
        //        // Generate PDF
        //        return GeneratePdf(bookings);
        //    }

        //    return BadRequest("Unsupported export format");
        //}







        [HttpGet]
        public async Task<IActionResult> BookingDetails(int id)
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

            ViewBag.StatusList = Enum.GetValues(typeof(CabBookingStatus))
                .Cast<CabBookingStatus>()
                .Select(s => new { Id = (int)s, Name = s.ToString() })
                .ToList();

            ViewBag.PaymentStatusList = Enum.GetValues(typeof(PaymentStatus))
                .Cast<PaymentStatus>()
                .Select(s => new { Id = (int)s, Name = s.ToString() })
                .ToList();

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBookingStatus(int id, CabBookingStatus status)
        {
            try
            {
                var booking = await _context.CabBookings
                    .Include(b => b.Cab)
                    .FirstOrDefaultAsync(b => b.CabBookingId == id);

                if (booking == null)
                {
                    TempData["Error"] = "Booking not found";
                    return RedirectToAction("ManageBookings");
                }

                // Validate status transition
                if (!IsValidStatusTransition(booking.Status, status))
                {
                    TempData["Error"] = $"Invalid status transition from {booking.Status} to {status}";
                    return RedirectToAction("BookingDetails", new { id });
                }

                var oldStatus = booking.Status;
                booking.Status = status;
                booking.UpdatedDate = DateTime.Now;
                booking.UpdatedBy = User.Identity.Name;

                // Update cab status based on booking status
                await UpdateCabStatus(booking.CabId, status, oldStatus);

                _context.CabBookings.Update(booking);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Booking status updated successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status");
                TempData["Error"] = "An error occurred while updating booking status";
            }

            return RedirectToAction("BookingDetails", new { id });
        }

        private bool IsValidStatusTransition(CabBookingStatus current, CabBookingStatus next)
        {
            // Define valid status transitions
            var validTransitions = new Dictionary<CabBookingStatus, List<CabBookingStatus>>
    {
        { CabBookingStatus.Pending, new() { CabBookingStatus.Confirmed, CabBookingStatus.Cancelled } },
        { CabBookingStatus.Confirmed, new() { CabBookingStatus.InProgress, CabBookingStatus.Cancelled } },
        { CabBookingStatus.InProgress, new() { CabBookingStatus.Completed, CabBookingStatus.Cancelled } },
        { CabBookingStatus.Completed, new() { } },
        { CabBookingStatus.Cancelled, new() { } }
    };

            return validTransitions[current].Contains(next);
        }

        private async Task UpdateCabStatus(int? cabId, CabBookingStatus bookingStatus, CabBookingStatus oldBookingStatus)
        {
            if (!cabId.HasValue) return;

            var cab = await _context.Cabs.FindAsync(cabId.Value);
            if (cab == null) return;

            switch (bookingStatus)
            {
                case CabBookingStatus.Cancelled:
                case CabBookingStatus.Completed:
                    // Only make cab available if it was actually booked
                    if (oldBookingStatus == CabBookingStatus.Confirmed ||
                        oldBookingStatus == CabBookingStatus.InProgress)
                    {
                        cab.Status = CabStatus.Available;
                    }
                    break;
                case CabBookingStatus.Confirmed:
                    cab.Status = CabStatus.Booked;
                    break;
            }

            _context.Cabs.Update(cab);
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> UpdateBookingStatus(int id, CabBookingStatus status)
        //{
        //    var booking = await _context.CabBookings.FindAsync(id);
        //    if (booking == null)
        //    {
        //        return NotFound();
        //    }

        //    var oldStatus = booking.Status;
        //    booking.Status = status;
        //    booking.UpdatedDate = DateTime.Now;
        //    booking.UpdatedBy = User.Identity.Name;

        //    // If booking is completed or cancelled, make the cab available again
        //    if ((status == CabBookingStatus.Completed || status == CabBookingStatus.Cancelled) &&
        //        (oldStatus != CabBookingStatus.Completed && oldStatus != CabBookingStatus.Cancelled))
        //    {
        //        var cab = await _context.Cabs.FindAsync(booking.CabId);
        //        if (cab != null)
        //        {
        //            cab.Status = CabStatus.Available;
        //            _context.Cabs.Update(cab);
        //        }
        //    }

        //    _context.CabBookings.Update(booking);
        //    await _context.SaveChangesAsync();

        //    TempData["Success"] = "Booking status updated successfully!";
        //    return RedirectToAction("BookingDetails", new { id });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus status)
        {
            var booking = await _context.CabBookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.PaymentStatus = status;
            if (status == PaymentStatus.Completed)
            {
                booking.PaymentDate = DateTime.Now;
            }
            booking.UpdatedDate = DateTime.Now;
            booking.UpdatedBy = User.Identity.Name;

            _context.CabBookings.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Payment status updated successfully!";
            return RedirectToAction("BookingDetails", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDriver(int id, string driverName, string driverPhone)
        {
            var booking = await _context.CabBookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.AssignedDriverName = driverName;
            booking.AssignedDriverPhone = driverPhone;
            booking.UpdatedDate = DateTime.Now;
            booking.UpdatedBy = User.Identity.Name;

            _context.CabBookings.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Driver assigned successfully!";
            return RedirectToAction("BookingDetails", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.CabBookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            booking.Status = CabBookingStatus.Cancelled;
            booking.UpdatedDate = DateTime.Now;
            booking.UpdatedBy = User.Identity.Name;

            // Make cab available again
            var cab = await _context.Cabs.FindAsync(booking.CabId);
            if (cab != null)
            {
                cab.Status = CabStatus.Available;
                _context.Cabs.Update(cab);
            }

            _context.CabBookings.Update(booking);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Booking cancelled successfully!";
            return RedirectToAction("ManageBookings");
        }
        #endregion
    }

    public class AdminCabDashboardViewModel
    {
        public int TotalCabs { get; set; }
        public int AvailableCabs { get; set; }
        public int TotalBookings { get; set; }
        public int TodayBookings { get; set; }
        public int PendingBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<CabBooking> RecentBookings { get; set; } = new();
    }


}






























//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using ToursAndTravelsManagement.Data;
//using ToursAndTravelsManagement.Enums;
//using ToursAndTravelsManagement.Models;

//namespace ToursAndTravelsManagement.Controllers
//{
//    [Authorize(Roles = "Admin")]
//    public class AdminCabController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public AdminCabController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // Manage Cabs
//        public async Task<IActionResult> ManageCabs()
//        {
//            var cabs = await _context.Cabs.ToListAsync();
//            return View(cabs);
//        }

//        // Manage Cities
//        public async Task<IActionResult> ManageCities()
//        {
//            var cities = await _context.Cities.ToListAsync();
//            return View(cities);
//        }

//        // Manage Routes
//        public async Task<IActionResult> ManageRoutes()
//        {
//            var routes = await _context.Routes
//                .Include(r => r.FromCity)
//                .Include(r => r.ToCity)
//                .ToListAsync();
//            return View(routes);
//        }

//        // Manage Cab Prices
//        public async Task<IActionResult> ManageCabPrices()
//        {
//            var cabPrices = await _context.CabRoutePrices
//                .Include(crp => crp.Cab)
//                .Include(crp => crp.Route)
//                .ThenInclude(r => r.FromCity)
//                .Include(crp => crp.Route)
//                .ThenInclude(r => r.ToCity)
//                .ToListAsync();
//            return View(cabPrices);
//        }

//        // View Bookings
//        public async Task<IActionResult> ViewBookings()
//        {
//            var bookings = await _context.CabBookings
//                .Include(b => b.CabRoutePrice)
//                .ThenInclude(crp => crp.Cab)
//                .Include(b => b.CabRoutePrice)
//                .ThenInclude(crp => crp.Route)
//                .ThenInclude(r => r.FromCity)
//                .Include(b => b.CabRoutePrice)
//                .ThenInclude(crp => crp.Route)
//                .ThenInclude(r => r.ToCity)
//                .OrderByDescending(b => b.CreatedDate)
//                .ToListAsync();
//            return View(bookings);
//        }

//        // Update Booking Status
//        [HttpPost]
//        public async Task<IActionResult> UpdateBookingStatus(int id, CabBookingStatus status)
//        {
//            var booking = await _context.CabBookings.FindAsync(id);
//            if (booking == null)
//            {
//                return NotFound();
//            }

//            booking.Status = status;
//            booking.UpdatedDate = DateTime.Now;
//            booking.UpdatedBy = User.Identity.Name;

//            if (status == CabBookingStatus.Completed || status == CabBookingStatus.Cancelled)
//            {
//                var cab = await _context.Cabs.FindAsync(booking.CabId);
//                if (cab != null)
//                {
//                    cab.Status = CabStatus.Available;
//                    _context.Cabs.Update(cab);
//                }
//            }

//            await _context.SaveChangesAsync();
//            TempData["Success"] = "Booking status updated successfully";
//            return RedirectToAction("ViewBookings");
//        }

//        // CRUD actions for Cabs, Cities, Routes, etc.
//        [HttpGet]
//        public IActionResult AddCab()
//        {
//            return View();
//        }

//        [HttpPost]
//        public async Task<IActionResult> AddCab(Cab cab)
//        {
//            if (ModelState.IsValid)
//            {
//                cab.CreatedDate = DateTime.Now;
//                cab.CreatedBy = User.Identity.Name;
//                _context.Cabs.Add(cab);
//                await _context.SaveChangesAsync();
//                TempData["Success"] = "Cab added successfully";
//                return RedirectToAction("ManageCabs");
//            }
//            return View(cab);
//        }

//    }
//}
