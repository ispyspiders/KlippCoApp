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

    public async Task<List<DateTime>> GetAvailableTimesAsync(string stylistId, Service service)
    {
        var schedule = await _context.StylistSchedule
            .Where(s => s.StylistId == stylistId && s.IsAvailable && s.Day >= DateTime.Today)
            .OrderBy(s => s.Day)
            .ToListAsync();

        var existingBookings = await _context.Bookings
            .Where(b => b.StylistId == stylistId && b.BookingTime.Date >= DateTime.Today)
            .ToListAsync();

        var availableTimes = new List<DateTime>();
        int serviceDuration = service.Duration;

        foreach (var day in schedule)
        {
            DateTime dayStart = day.Day.Date.Add(day.StartTime);
            DateTime dayEnd = day.Day.Date.Add(day.EndTime);
            DateTime breakStart = day.Day.Date.Add(day.BreakStart);
            DateTime breakEnd = day.Day.Date.Add(day.BreakEnd);
            TimeSpan buffer = day.BufferTime;

            DateTime current = dayStart;

            // Avrunda till hel min
            current = current.AddTicks(-(current.Ticks % TimeSpan.TicksPerMinute));

            while (current.AddMinutes(serviceDuration) <= dayEnd)
            {
                // Kontrollera om tiden överlappar med paus
                bool onBreak = current < breakEnd && current.AddMinutes(serviceDuration) > breakStart;

                // Kontrollera om tiden redan är bokad (med tidsintervall)
                bool alreadyBooked = existingBookings.Any(b =>
                    b.BookingTime < current.AddMinutes(serviceDuration) &&
                    b.BookingTime.AddMinutes(serviceDuration) > current);

                // Endast framtida tider som inte ligger på paus eller är bokade
                if (current > DateTime.Now && !onBreak && !alreadyBooked)
                {
                    availableTimes.Add(current);
                }

                // Stega framåt med tjänstens längd + buffert
                current = current.AddMinutes(serviceDuration + buffer.TotalMinutes);
            }
        }

        return availableTimes;
    }


    // // Skapa en bokning om tiden är ledig
    public async Task<bool> CreateBookingAsync(Booking booking)
    {
        // Nollställ sekunder och millisekunder
        booking.BookingTime = new DateTime(
                booking.BookingTime.Year,
                booking.BookingTime.Month,
                booking.BookingTime.Day,
                booking.BookingTime.Hour,
                booking.BookingTime.Minute,
                0);

        // Hämta tjänst som bokningen gäller – kontroll att tjänsten finns
        var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == booking.ServiceId);
        if (service == null) return false;

        // Tjänstens längd
        int serviceDuration = service.Duration;

        // Hämta alla existerande bokningar för samma stylist och samma datum
        var existingBookings = await _context.Bookings
         .Include(b => b.Service)
         .Where(b => b.StylistId == booking.StylistId &&
                     b.BookingTime.Date == booking.BookingTime.Date)
         .ToListAsync();

        // Hämta alla tjänster (fallback om .Include(b=>b.Service) inte räcker)
        var allServices = await _context.Service.ToListAsync();

        // Flagga för att markera om någon tid krockar
        bool isBooked = false;

        // Krockkontroll
        foreach (var b in existingBookings)
        {
            var bStart = b.BookingTime;
            // Hämta aktuell boknings längd via include eller fallback
            var bDuration = b.Service?.Duration ?? allServices.FirstOrDefault(s => s.Id == b.ServiceId)?.Duration ?? 0;

            if (bDuration == 0) Console.WriteLine($"[VARNING] Tjänst saknar duration. BookingId: {b.Id}, ServiceId: {b.ServiceId}");

            // Beräkna slutet på befintlig bokning
            var bEnd = bStart.AddMinutes(bDuration);

            // Beräkna start/slut på önskad bokning
            var reqStart = booking.BookingTime;
            var reqEnd = reqStart.AddMinutes(serviceDuration);

            // Kontroll om överlappning mellan tider
            if (bStart < reqEnd && bEnd > reqStart)
            {
                // Om krock avbryt
                Console.WriteLine($"[KROCK] {reqStart:HH:mm} - {reqEnd:HH:mm} krockar med befintlig {bStart:HH:mm} - {bEnd:HH:mm}");
                isBooked = true;
                break;
            }
        }

        // Om bokad avbryt
        if (isBooked) return false;

        // Hämta schemat för dagen
        var schedule = await _context.StylistSchedule
       .FirstOrDefaultAsync(s =>
           s.StylistId == booking.StylistId &&
           s.Day.Date == booking.BookingTime.Date &&
           s.IsAvailable);
        if (schedule == null) return false;

        // Kontrollera att tiden ryms inom schemat + inte krockar med rast
        var startTime = booking.BookingTime;
        var endTime = booking.BookingTime.AddMinutes(serviceDuration);
        var fullStart = schedule.Day.Date.Add(schedule.StartTime);
        var fullEnd = schedule.Day.Date.Add(schedule.EndTime);
        var breakStart = schedule.Day.Date.Add(schedule.BreakStart);
        var breakEnd = schedule.Day.Date.Add(schedule.BreakEnd);
        bool isWithinSchedule = startTime >= fullStart && endTime <= fullEnd;
        bool overlapsBreak = startTime < breakEnd && endTime > breakStart;

        if (!isWithinSchedule || overlapsBreak) return false;

        // Om allt OK, spara bokning
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();

        return true;
    }
}
