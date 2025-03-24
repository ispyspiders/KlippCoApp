using KlippCoApp.Data;
using KlippCoApp.Models;
using Microsoft.EntityFrameworkCore;

public class BookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Hämta alla lediga tider för en stylist på ett specifikt datum
    public async Task<List<StylistSchedule>> GetAvailableTimes(string stylistId, DateTime date)
    {
        var schedules = await _context.StylistSchedule
            .Where(s => s.StylistId == stylistId && s.Day.Date == date.Date && s.IsAvailable)
            .ToListAsync();

        return schedules;
    }

    // Skapa en bokning om tiden är ledig
    public async Task<bool> CreateBookingAsync(Booking booking)
    {
        // Kontrollera om den valda tiden är ledig
        var existingBooking = await _context.Bookings
            .AnyAsync(b => b.StylistScheduleId == booking.StylistScheduleId && b.BookingTime == booking.BookingTime);

        if (existingBooking)
        {
            return false; // Om tiden redan är bokad
        }

        // Markera den aktuella schematiden som inte tillgänglig
        var stylistSchedule = await _context.StylistSchedule
            .FirstOrDefaultAsync(s => s.Id == booking.StylistScheduleId);

        if (stylistSchedule != null)
        {
            stylistSchedule.IsAvailable = false; // Gör tiden otillgänglig för bokning
            _context.Update(stylistSchedule); // Uppdatera i databasen
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return true;
    }
}
