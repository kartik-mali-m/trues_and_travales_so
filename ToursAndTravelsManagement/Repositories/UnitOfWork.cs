using Microsoft.EntityFrameworkCore;
using ToursAndTravelsManagement.Data;
using ToursAndTravelsManagement.Models;
using ToursAndTravelsManagement.Repositories.IRepositories;

namespace ToursAndTravelsManagement.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IGenericRepository<Booking> _bookingRepository;
    private IGenericRepository<Tour> _tourRepository;
    private IGenericRepository<Destination> _destinationRepository;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<Booking> BookingRepository
    {
        get
        {
            if (_bookingRepository == null)
            {
                _bookingRepository = new GenericRepository<Booking>(_context);
            }
            return _bookingRepository;
        }
    }

    public IGenericRepository<Tour> TourRepository
    {
        get
        {
            if (_tourRepository == null)
            {
                _tourRepository = new GenericRepository<Tour>(_context);
            }
            return _tourRepository;
        }
    }

    public IGenericRepository<Destination> DestinationRepository
    {
        get
        {
            if (_destinationRepository == null)
            {
                _destinationRepository = new GenericRepository<Destination>(_context);
            }
            return _destinationRepository;
        }
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}