using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Security.Claims;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;
using ToursAndTravelsManagement.Services.EmailService;
using ToursAndTravelsManagement.Services.PdfService;

namespace ToursAndTravelsManagement.Controllers;

[Authorize(Policy = "RequireCustomerRole")] // Allow authenticated users
public class UserBookingsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IPdfService _pdfService;


    public UserBookingsController(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IPdfService pdfService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    // GET: UserBookings/AvailableTours
    public async Task<IActionResult> AvailableTours()
    {
        var tours = await _unitOfWork.TourRepository.GetAllAsync();
        return View(tours);
    }

    // GET: UserBookings/BookTour/5
    public async Task<IActionResult> BookTour(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(id.Value);
        if (tour == null)
        {
            return NotFound();
        }

        ViewBag.Tour = tour;
        return View();
    }

    // POST: UserBookings/BookTour/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookTour([Bind("TourId,BookingDate,NumberOfParticipants,PaymentMethod")] Booking booking)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        booking.UserId = currentUser.Id;

        var tour = await _unitOfWork.TourRepository.GetByIdAsync(booking.TourId);
        if (tour == null)
        {
            return NotFound("Selected tour not found.");
        }

        booking.TotalPrice = tour.Price * booking.NumberOfParticipants;

        if (ModelState.IsValid)
        {
            await _unitOfWork.BookingRepository.AddAsync(booking);
            await _unitOfWork.CompleteAsync();

            // Generate a ticket after booking is confirmed
            var ticket = new Ticket
            {
                TicketNumber = Guid.NewGuid().ToString().Substring(0, 8), // Random ticket number
                CustomerName = currentUser.UserName,
                TourName = tour.Name,
                BookingDate = DateTime.Now,
                TourStartDate = tour.StartDate,
                TourEndDate = tour.EndDate,
                TotalPrice = booking.TotalPrice
            };

            // Generate the PDF for the ticket
            var pdf = _pdfService.GenerateTicketPdf(ticket);

            // Send the PDF via email
            await _emailService.SendTicketEmailAsync(currentUser.Email, $"Your Ticket - {ticket.TicketNumber}", "Thank you for booking! Please find your ticket attached.", pdf);

            return RedirectToAction("MyBookings", "UserBookings"); // Redirects to user's booking list
        }
        return View(booking);
    }


    // GET: UserBookings/MyBookings
    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        // Fetch bookings and include related data (Tour)
        var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
            b => b.UserId == currentUser.Id, // Only fetch bookings for the current user
            includeProperties: "Tour" // Include the related Tour entity
        );

        return View(bookings);
    }

    // POST: UserBookings/MyBookings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MyBookings(int bookingId, string action)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (action == "Cancel")
        {
            var booking = await _unitOfWork.BookingRepository.GetByIdAsync(bookingId);
            if (booking == null || booking.UserId != currentUser.Id)
            {
                return NotFound();
            }

            // Check if the booking is already canceled
            if (booking.Status == BookingStatus.Canceled)
            {
                return BadRequest("Booking is already canceled.");
            }

            // Update booking status to canceled
            booking.Status = BookingStatus.Canceled;
            booking.IsActive = false;

            _unitOfWork.BookingRepository.Update(booking);
            await _unitOfWork.CompleteAsync();

            // Redirect to the MyBookings view
            return RedirectToAction("MyBookings");
        }

        // Fallback case, if action is not recognized
        return BadRequest("Invalid action.");
    }

    //[HttpGet]
    //public async Task<IActionResult> ExportBookingsPdf()
    //{
    //    var currentUser = await _userManager.GetUserAsync(User);
    //    var userName = User?.Identity?.Name ?? "Unknown User"; // Get the current logged-in user

    //    Log.Information("User {UserName} is exporting their bookings to PDF", userName);

    //    // Fetch bookings and include related data (Tour)
    //    var bookings = await _unitOfWork.BookingRepository.GetAllAsync(
    //        b => b.UserId == currentUser.Id, // Only fetch bookings for the current user
    //        includeProperties: "Tour" // Include the related Tour entity
    //    );

    //    if (bookings == null || !bookings.Any())
    //    {
    //        Log.Warning("User {UserName} tried to export bookings to PDF, but no bookings were found", userName);
    //        return NotFound("No bookings found to export.");
    //    }

    //    // Convert IEnumerable to List
    //    var bookingsList = bookings.ToList();

    //    // Generate the PDF using the PDF service
    //    var pdf = _pdfService.GenerateBookingsPdf(bookingsList);

    //    return File(pdf, "application/pdf", "BookingsReport.pdf");
    //}


}
