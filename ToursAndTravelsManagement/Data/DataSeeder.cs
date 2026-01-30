using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Data
{
    public class DataSeeder
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public DataSeeder(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task SeedDestinationsAsync(int numberOfDestinations)
        {
            // Using Bogus to generate fake data for destinations
            var faker = new Faker<Destination>()
                .RuleFor(d => d.Name, f => f.Address.City())
                .RuleFor(d => d.Description, f => f.Lorem.Paragraph())
                .RuleFor(d => d.Country, f => f.Address.Country())
                .RuleFor(d => d.City, f => f.Address.City())
                .RuleFor(d => d.ImageUrl, f => f.Internet.Url())
                .RuleFor(d => d.CreatedBy, f => f.Person.FullName)
                .RuleFor(d => d.CreatedDate, f => f.Date.Past(1))
                .RuleFor(d => d.IsActive, f => f.Random.Bool());

            // Generate a list of fake destinations
            var destinations = faker.Generate(numberOfDestinations);

            // Add the generated destinations to the database
            foreach (var destination in destinations)
            {
                await _unitOfWork.DestinationRepository.AddAsync(destination);
            }

            // Save changes to the database
            await _unitOfWork.CompleteAsync();
        }

        public async Task SeedToursAsync(int numberOfTours)
        {
            var random = new Random();

            // Fetch all destination IDs to link the tours to existing destinations
            var destinations = await _unitOfWork.DestinationRepository.GetAllAsync();
            var destinationIds = destinations.Select(d => d.DestinationId).ToList();

            // Ensure we have at least one destination
            if (!destinationIds.Any())
            {
                throw new InvalidOperationException("No destinations found in the database.");
            }

            // Using Bogus to generate fake data for Tours
            var faker = new Faker<Tour>()
                .RuleFor(t => t.Name, f => f.Lorem.Sentence(3))
                .RuleFor(t => t.Description, f => f.Lorem.Paragraph())
                .RuleFor(t => t.StartDate, f => f.Date.Future(0)) // Future date
                .RuleFor(t => t.EndDate, (f, t) => t.StartDate.AddDays(f.Random.Int(2, 14))) // EndDate after StartDate
                .RuleFor(t => t.Price, f => f.Random.Decimal(100, 1000)) // Random price between 100 and 1000
                .RuleFor(t => t.MaxParticipants, f => f.Random.Int(5, 50)) // Participants between 5 and 50
                .RuleFor(t => t.DestinationId, f => f.PickRandom(destinationIds)) // Link to a random destination
                .RuleFor(t => t.CreatedBy, f => f.Person.FullName)
                .RuleFor(t => t.CreatedDate, f => f.Date.Past(1))
                .RuleFor(t => t.IsActive, f => f.Random.Bool());

            // Generate the specified number of fake tours
            var tours = faker.Generate(numberOfTours);

            // Add the generated tours to the database
            foreach (var tour in tours)
            {
                await _unitOfWork.TourRepository.AddAsync(tour);
            }

            // Save changes to the database
            await _unitOfWork.CompleteAsync();
        }

        public async Task SeedBookingsAsync(int numberOfBookings)
        {
            var random = new Random();

            // Fetch all users and tours to link the bookings
            var users = await _userManager.Users.ToListAsync();  // Make sure using Microsoft.EntityFrameworkCore is present
            var tours = await _unitOfWork.TourRepository.GetAllAsync();

            var userIds = users.Select(u => u.Id).ToList();
            var tourIds = tours.Select(t => t.TourId).ToList();

            if (!userIds.Any() || !tourIds.Any())
            {
                throw new InvalidOperationException("No users or tours found in the database.");
            }

            // Using Bogus to generate fake data for Bookings
            var faker = new Faker<Booking>()
                .RuleFor(b => b.UserId, f => f.PickRandom(userIds))
                .RuleFor(b => b.TourId, f => f.PickRandom(tourIds))
                .RuleFor(b => b.BookingDate, f => f.Date.Recent(30))
                .RuleFor(b => b.NumberOfParticipants, f => f.Random.Int(1, 5))
                .RuleFor(b => b.TotalPrice, (f, b) => b.NumberOfParticipants * f.Random.Decimal(100, 1000))
                .RuleFor(b => b.Status, f => f.PickRandom<BookingStatus>())
                .RuleFor(b => b.PaymentMethod, f => f.PickRandom(new[] { "Credit Card", "Debit Card", "PayPal" }))
                .RuleFor(b => b.PaymentStatus, f => f.PickRandom<PaymentStatus>())
                .RuleFor(b => b.CreatedBy, f => f.Person.FullName)
                .RuleFor(b => b.CreatedDate, f => f.Date.Past(1))
                .RuleFor(b => b.IsActive, f => f.Random.Bool());

            var bookings = faker.Generate(numberOfBookings);

            foreach (var booking in bookings)
            {
                await _unitOfWork.BookingRepository.AddAsync(booking);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}
