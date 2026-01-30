using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Enums;
using ToursAndTravelsManagement.Models;
using Route = ToursAndTravelsManagement.Models.Route;

namespace ToursAndTravelsManagement.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Tour> Tours { get; set; }
    public DbSet<Destination> Destinations { get; set; }



    public DbSet<Cab> Cabs { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<CabRoutePrice> CabRoutePrices { get; set; }
    public DbSet<CabBooking> CabBookings { get; set; }
    public DbSet<CabBookingInvoice> CabBookingInvoices { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Additional configurations if needed

        modelBuilder.Entity<CabBooking>()
        .HasOne(b => b.CabBookingInvoice)
        .WithOne(i => i.CabBooking)
        .HasForeignKey<CabBookingInvoice>(i => i.CabBookingId)
        .OnDelete(DeleteBehavior.Cascade);


        // Configure relationships
        modelBuilder.Entity<CabBooking>()
            .HasOne(cb => cb.CabRoutePrice)
            .WithMany()
            .HasForeignKey(cb => cb.CabRoutePriceId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CabBooking>()
            .HasOne(cb => cb.Cab)
            .WithMany()
            .HasForeignKey(cb => cb.CabId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Route>()
            .HasOne(r => r.FromCity)
            .WithMany()
            .HasForeignKey(r => r.FromCityId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Route>()
            .HasOne(r => r.ToCity)
            .WithMany()
            .HasForeignKey(r => r.ToCityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique constraints
        modelBuilder.Entity<Cab>()
            .HasIndex(c => c.RegistrationNumber)
            .IsUnique();

        modelBuilder.Entity<City>()
            .HasIndex(c => new { c.Name, c.State })
            .IsUnique();

        modelBuilder.Entity<Route>()
            .HasIndex(r => new { r.FromCityId, r.ToCityId })
            .IsUnique();

        modelBuilder.Entity<CabRoutePrice>()
            .HasIndex(crp => new { crp.CabId, crp.RouteId, crp.JourneyType })
            .IsUnique();

        modelBuilder.Entity<CabBooking>()
            .HasIndex(cb => cb.BookingNumber)
            .IsUnique();

        // Seed data for Cities
        modelBuilder.Entity<City>().HasData(
            new City
            {
                CityId = 1,
                Name = "Pune",
                State = "Maharashtra",
                Country = "India",
                IsPopular = true,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new City
            {
                CityId = 2,
                Name = "Mumbai",
                State = "Maharashtra",
                Country = "India",
                IsPopular = true,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new City
            {
                CityId = 3,
                Name = "Goa",
                State = "Goa",
                Country = "India",
                IsPopular = true,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new City
            {
                CityId = 4,
                Name = "Nagpur",
                State = "Maharashtra",
                Country = "India",
                IsPopular = true,
                CreatedDate = DateTime.Now,
                IsActive = true
            }
        );

        // Seed data for Cabs
        modelBuilder.Entity<Cab>().HasData(
            new Cab
            {
                CabId = 1,
                Name = "SWIFT DZIRE",
                Type = CabType.Sedan,
                Model = "2023",
                RegistrationNumber = "MH12AB1234",
                Year = 2023,
                HasAC = true,
                SeatingCapacity = 4,
                BasePricePerKM = 15,
                Features = "[\"Music System\", \"Airbags\", \"Power Windows\"]",
                Status = CabStatus.Available,
                CreatedDate = DateTime.Now,
                IsActive = true
            },
            new Cab
            {
                CabId = 2,
                Name = "TOYOTA INNOVA",
                Type = CabType.MUV,
                Model = "2022",
                RegistrationNumber = "MH12CD5678",
                Year = 2022,
                HasAC = true,
                SeatingCapacity = 7,
                BasePricePerKM = 25,
                Features = "[\"Rear AC\", \"Leather Seats\", \"Entertainment System\"]",
                Status = CabStatus.Available,
                CreatedDate = DateTime.Now,
                IsActive = true
            }
        );
    }
}
