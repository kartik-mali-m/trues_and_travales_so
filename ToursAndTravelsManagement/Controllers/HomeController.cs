using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.ViewModels;

namespace ToursAndTravelsManagement.Controllers;

public class HomeController : Controller
{
    private readonly IEmailService _emailService;
    private readonly ApplicationDbContext _context;


    public HomeController(IEmailService emailService, ApplicationDbContext context)
    {
        _emailService = emailService;
        _context = context;
    }
    // GET: Home/Index
    public async Task<IActionResult> Index()
    {
        var viewModel = new HomePageViewModel
        {
            // Get popular cities
            PopularCities = await _context.Cities
                .Where(c => c.IsActive && c.IsPopular)
                .OrderBy(c => c.Name)
                .Take(6)
                .ToListAsync(),

            // Get featured cabs (available, with AC, and high rating)
            FeaturedCabs = await _context.Cabs
                .Where(c => c.IsActive &&
                            c.Status == CabStatus.Available &&
                            c.HasAC)
                .OrderByDescending(c => c.SeatingCapacity)
                .Take(4)
                .ToListAsync(),

            // Get popular routes
            PopularRoutes = await _context.Routes
                .Include(r => r.FromCity)
                .Include(r => r.ToCity)
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.Distance)
                .Take(4)
                .ToListAsync(),

            // Get statistics
            TotalCabs = await _context.Cabs.CountAsync(c => c.IsActive),
            TotalBookings = await _context.CabBookings.CountAsync(b => b.IsActive),

            // Initialize search model
            SearchModel = new CabSearchViewModel()
        };

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Search(CabSearchViewModel model)
    {
        // Redirect to CabController SearchResults
        return RedirectToAction("SearchResults", "Cab", model);
    }


    public async Task<IActionResult> About()
    {
        ViewData["TotalCabs"] = await _context.Cabs.CountAsync(c => c.IsActive);
        ViewData["TotalCities"] = await _context.Cities.CountAsync(c => c.IsActive);
        ViewData["TotalBookings"] = await _context.CabBookings.CountAsync(b => b.IsActive);
        ViewData["SatisfiedCustomers"] = await _context.CabBookings
            .Where(b => b.Status == CabBookingStatus.Completed)
            .Select(b => b.UserId)
            .Distinct()
            .CountAsync();

        return View();
    }



    public IActionResult Contact()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> GetCitySuggestions(string term)
    {
        var cities = await _context.Cities
            .Where(c => c.IsActive && c.Name.Contains(term))
            .Select(c => new { id = c.CityId, text = c.Name })
            .Take(10)
            .ToListAsync();

        return Json(cities);
    }


    [HttpGet]
    public async Task<IActionResult> QuickSearch(string fromCity, string toCity, string journeyType)
    {
        // Parse journey type
        JourneyType journey = JourneyType.OneWay;
        if (!string.IsNullOrEmpty(journeyType))
        {
            Enum.TryParse<JourneyType>(journeyType, out journey);
        }

        var model = new CabSearchViewModel
        {
            FromCity = fromCity,
            ToCity = toCity,
            TravelDate = DateTime.Now.AddDays(1),
            JourneyType = journey
        };

        return RedirectToAction("SearchResults", "Cab", model);
    }

    //[HttpGet]
    //public IActionResult About()
    //{
    //    ViewData["Title"] = "About Us";
    //    ViewData["Description"] = "Learn about Tours & Travels Management - your trusted partner for unforgettable journeys since 2010.";
    //    ViewData["Keywords"] = "about us, travel company, tours about, travel agency, our story";

    //    // You can pass any model data if needed
    //    // var aboutData = GetAboutData();
    //    // return View(aboutData);

    //    return View();
    //}


    // GET: Home/Privacy
    public IActionResult Privacy()
    {
        Log.Information("Privacy page accessed by user {UserId} at {Timestamp}", User.Identity?.Name, DateTime.Now);
        return View();
    }

    // GET: Home/Error
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        Log.Error("Error page accessed. RequestId: {RequestId}, User: {UserId}, Timestamp: {Timestamp}", requestId, User.Identity?.Name, DateTime.Now);

        return View(new ErrorViewModel { RequestId = requestId });
    }

    [HttpPost]
    public async Task<IActionResult> SubmitForm(ContactFormModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model); // Return to form view with model state
        }

        // Load the email template
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "emailTemplates", "emailTemplate.html");
        var emailTemplate = await System.IO.File.ReadAllTextAsync(templatePath);

        // Replace placeholders with actual values
        emailTemplate = emailTemplate.Replace("{{FirstName}}", model.FirstName)
                                     .Replace("{{LastName}}", model.LastName)
                                     .Replace("{{Email}}", model.Email)
                                     .Replace("{{Subject}}", model.Subject)
                                     .Replace("{{Message}}", model.Message);

        // Send email
        await _emailService.SendEmailAsync(model.Email, model.Subject, emailTemplate);

        return RedirectToAction("ThankYou");
    }

    public IActionResult ThankYou()
    {
        return View();
    }
}
