using System.Security.Claims;
using System.Threading.Tasks;
using KlippCoApp.Data;
using KlippCoApp.Models;
using Microsoft.AspNetCore.Authorization;
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

    // Hjälpmetod för att hämta alla stylister
    private async Task<List<ApplicationUser>> GetAllStylistsAsync()
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
        return stylists;
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
        return RedirectToAction("SelectStylist", new { serviceId });
    }

    // Steg 2: Välj utövare
    [HttpGet]
    public async Task<IActionResult> SelectStylist(int serviceId)
    {
        var stylists = await GetAllStylistsAsync();

        // Läs in och skicka service
        ViewBag.Service = await _context.Service.FindAsync(serviceId);

        // Lägg till "valfri utövare"
        stylists.Insert(0, new ApplicationUser { Id = "", Firstname = "Valfri utövare" });

        // Returnera vyn med listan av stylister
        return View(stylists);
    }

    [HttpPost]
    public async Task<IActionResult> SelectStylist(int serviceId, string stylistId)
    {
        return RedirectToAction("SelectTime", new { serviceId, stylistId });
    }


    // Steg 3: Välj bland lediga tider
    [HttpGet]
    public async Task<IActionResult> SelectTime(string stylistId, int serviceId)
    {
        var service = await _context.Service.FirstOrDefaultAsync(s => s.Id == serviceId);
        if (service == null) return NotFound();

        var events = new List<object>();

        if (string.IsNullOrEmpty(stylistId))
        {
            // "Valfri utövare" vald, hämta alla tider för alla stylister
            var stylists = await GetAllStylistsAsync();
            foreach (var stylist in stylists)
            {
                var times = await _bookingService.GetAvailableTimesAsync(stylist.Id, service);
                foreach (var t in times)
                {
                    events.Add(new
                    {
                        title = $"{service.Name} – {stylist.Firstname}",
                        start = t.ToString("s"),
                        allDay = false,
                        extendedProps = new { stylistId = stylist.Id, stylistName = stylist.Firstname }
                    });
                }
            }
        }
        else
        {
            // Specifik stylist vald
            var stylist = await _context.Users.FindAsync(stylistId);
            if (stylist == null) return NotFound();
            var times = await _bookingService.GetAvailableTimesAsync(stylist.Id, service);
            foreach (var t in times)
            {
                events.Add(new
                {
                    title = $"{service.Name} – {stylist.Firstname}",
                    start = t.ToString("s"),
                    allDay = false,
                    extendedProps = new { stylistId = stylist.Id, stylistName = stylist.Firstname }
                });
            }
        }
        ViewBag.Events = events;
        ViewBag.Service = service;
        ViewBag.ServiceId = service.Id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SelectTime(string stylistId, int serviceId, string bookingTime)
    {
        var stylist = await _context.Users.FindAsync(stylistId);
        var service = await _context.Service.FindAsync(serviceId);

        if (stylist == null || service == null)
        {
            return BadRequest();
        }

        if (!DateTimeOffset.TryParse(bookingTime, out var parsedOffsetTime))
        {
            return BadRequest();
        }

        var model = new BookingViewModel
        {
            ServiceId = service.Id,
            ServiceName = service.Name,
            StylistId = stylistId,
            StylistName = stylist.Firstname,
            BookingTime = parsedOffsetTime // ← använd direkt
        };

        return View("ConfirmBooking", model);
    }

    // Steg 4: Bekräfta bokning
    [HttpGet]
    public IActionResult ConfirmBooking(int serviceId, string stylistId, string bookingTime)
    {
        if (!DateTimeOffset.TryParse(bookingTime, out var parsedTimeOffset))
        {
            return BadRequest("Ogiltig tid");
        }

        var stylist = _context.Users.Find(stylistId);
        var service = _context.Service.Find(serviceId);

        if (stylist == null || service == null)
        {
            return NotFound();
        }

        var model = new BookingViewModel
        {
            ServiceId = service.Id,
            ServiceName = service.Name,
            StylistId = stylist.Id,
            StylistName = stylist.Firstname,
            BookingTime = parsedTimeOffset
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBooking(BookingViewModel model)
    {
        if (!ModelState.IsValid) return View("ConfirmBooking", model);

        // Om användare inte är inloggad skicka till logga in
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("ConfirmBooking", model) });
        }

        // Om inloggad
        var booking = new Booking
        {
            CustomerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
            StylistId = model.StylistId,
            ServiceId = model.ServiceId,
            BookingTime = model.BookingTime.LocalDateTime
        };

        var success = await _bookingService.CreateBookingAsync(booking);

        if (success) return RedirectToAction("BookingConfirmation", new { id = booking.Id });

        ModelState.AddModelError("", "Tiden är redan bokad");
        return View("ConfirmBooking", model);
    }


    // Visa bokningsbekräftelse
    [HttpGet]
    public async Task<IActionResult> BookingConfirmation(int id)
    {
        var booking = await _context.Bookings
                .Include(b => b.Service)
                .Include(b => b.Stylist)
                .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();

        // Skapa ViewModel 
        var model = new BookingViewModel
        {
            ServiceId = booking.ServiceId,
            ServiceName = booking.Service?.Name,
            StylistId = booking.StylistId,
            StylistName = booking.Stylist?.Firstname,
            BookingTime = booking.BookingTime
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Stylist")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteBooking(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);

        if (booking == null) return NotFound();

        _context.Bookings.Remove(booking);
        await _context.SaveChangesAsync();

        return Ok();
    }
}
