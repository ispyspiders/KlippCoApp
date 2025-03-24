using System.Security.Claims;
using KlippCoApp.Data;
using KlippCoApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BookingController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly BookingService _bookingService;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<BookingController> _logger;

    public BookingController(ApplicationDbContext context, BookingService bookingService, UserManager<ApplicationUser> userManager, ILogger<BookingController> logger)
    {
        _context = context;
        _bookingService = bookingService;
        _userManager = userManager;
        _logger = logger;
    }



    // Steg 1: Välj tjänst
    [HttpGet]
    public async Task<IActionResult> SelectService()
    {
        var services = await _context.Service.ToListAsync();
        return View(services);
    }

    [HttpPost]
    public async Task<IActionResult> SelectService(int serviceId)
    {
        return RedirectToAction("SelectStylist", new { serviceId = serviceId });
    }

    // Steg 2: Välj utövare
    [HttpGet]
    public async Task<IActionResult> SelectStylist(int serviceId)
    {
        // Hämta alla användare som har rollen "Stylist"
        var users = await _context.Users.ToListAsync();
        var stylists = new List<ApplicationUser>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Stylist"))
            {
                stylists.Add(user);
            }
        }

        // Hämta tjänsten baserat på serviceId (den tjänst som valdes på föregående steg)
        var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == serviceId);

        // Skicka den valda tjänsten till vyn via ViewBag
        ViewBag.Service = service;

        // Returnera vyn med listan av stylister
        return View(stylists);
    }

    [HttpPost]
    public async Task<IActionResult> SelectStylist(int serviceId, string stylistId)
    {
        return RedirectToAction("SelectTime", new { serviceId = serviceId, stylistId = stylistId });
    }


    // Steg 3: Välj bland lediga tider
    [HttpGet]
    public async Task<IActionResult> SelectTime(string stylistId, int serviceId)
    {
        var stylist = await _context.Users.FirstOrDefaultAsync(u => u.Id == stylistId);
        var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == serviceId);

        if (stylist == null || service == null) return NotFound();

        // Hämta lediga tider baserat på stylistens schema
        var availableTimes = await GetAvailableTimesAsync(stylist, service);

        // Skapa lista med lediga tider i ett format FullCalendar kan förstå
        var events = availableTimes.Select(time => new
        {
            title = $"{service.Name} - {stylist.Firstname}",
            start = time.ToString("yyyy-MM-ddTHH:mm:ss"),
            end = time.AddMinutes(service.Duration).ToString("yyyy-MM-ddTHH:mm:ss")
        }).ToList();

        ViewBag.Service = service;
        ViewBag.Stylist = stylist;
        ViewBag.Events = events;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SelectTime(string stylistId, int serviceId, string bookingTime)
    {
        var stylist = await _context.Users.FirstOrDefaultAsync(u => u.Id == stylistId);
        var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == serviceId);

        if (stylist == null || service == null) return NotFound();

        if (DateTime.TryParse(bookingTime, out DateTime selectedBookingTime))
        {
            var model = new BookingViewModel
            {
                ServiceId = serviceId,
                StylistId = stylistId,
                ServiceName = service.Name,
                StylistName = stylist.Firstname,
                BookingTime = selectedBookingTime
            };
            return View("ConfirmBooking", model);
        }
        else
        {
            ModelState.AddModelError("", "Ogiltig tid vald.");
            return View();
        }


        // return RedirectToAction("ConfirmBooking", new { serviceId = serviceId, stylistId = stylistId, bookingTime = bookingTime });
    }

    // Steg 4: Bekräfta bokning
    [HttpGet]
    public async Task<IActionResult> ConfirmBooking(BookingViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction("SelectTime");

        return View(model); // Rendera bekräftelsesidan för användaren
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(BookingViewModel model)
    {
        if (ModelState.IsValid)
        {

            // Hämta stylist och tjänst
            var stylist = await _context.Users.FirstOrDefaultAsync(u => u.Id == model.StylistId);
            var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == model.ServiceId);

            if (stylist == null || service == null)
            {
                ModelState.AddModelError("", "Tjänst eller stylist finns inte");
                return View("ConfirmBooking", model);
            }

            // SKapa bokning
            var booking = new Booking
            {
                CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                StylistId = model.StylistId,
                ServiceId = model.ServiceId,
                BookingTime = model.BookingTime
            };

            var result = await _bookingService.CreateBookingAsync(booking);
            if (result)
            {
                return RedirectToAction("BookingConfirmation", new { id = booking.Id });
            }

            ModelState.AddModelError("", "Tiden är redan bokad. Välj en annan tid.");
            return View("ConfirmBooking", model);

            // _context.Add(booking);
            // await _context.SaveChangesAsync();
        }
        else return View("ConfirmBooking");
    }

    private async Task<List<DateTime>> GetAvailableTimesAsync(ApplicationUser stylist, Service service)
    {
        var schedule = await _context.StylistSchedule
                                     .Where(s => s.StylistId == stylist.Id && s.IsAvailable)
                                     .OrderBy(s => s.Day) // Hämta alla tillgängliga scheman i framtiden
                                     .ToListAsync();

        if (schedule == null || !schedule.Any()) return new List<DateTime>(); // Om inget schema finns, returnera en tom lista

        var existingBookings = await _context.Bookings
            .Where(b => b.StylistId == stylist.Id && b.BookingTime.Date >= DateTime.Today)
            .ToListAsync();

        List<DateTime> availableTimes = new List<DateTime>();

        int serviceDurationInMin = service.Duration;

        // Loop genom alla scheman för stylisten
        foreach (var daySchedule in schedule)
        {
            DateTime currentTime = daySchedule.Day.Date.Add(daySchedule.StartTime);

            while (currentTime.AddMinutes(serviceDurationInMin) <= daySchedule.Day.Date.Add(daySchedule.EndTime))
            {
                // Filtrera bort redan passerade tider
                if (currentTime <= DateTime.Now)
                {
                    currentTime = currentTime.AddMinutes(serviceDurationInMin + daySchedule.BufferTime.TotalMinutes);
                    continue; // Om tiden har passerat, hoppa till nästa tillgängliga tid
                }

                // Kolla om tiden är bokad
                bool isBooked = existingBookings.Any(b => b.BookingTime == currentTime);

                if (!isBooked)
                {
                    availableTimes.Add(currentTime);
                }

                // Lägg till buffertid mellan bokningar
                currentTime = currentTime.AddMinutes(serviceDurationInMin + daySchedule.BufferTime.TotalMinutes);
            }
        }

        return availableTimes;
    }



    // Visa bokningsbekräftelse
    [HttpGet]
    public IActionResult BookingConfirmation(BookingViewModel model)
    {
        return View();
    }
}
