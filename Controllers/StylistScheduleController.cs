using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KlippCoApp.Data;
using KlippCoApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace KlippCoApp.Controllers
{
    [Authorize(Roles = "Admin,Stylist")]
    public class StylistScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public StylistScheduleController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: StylistSchedule
        public async Task<IActionResult> Index(string selectedStylistId)
        {
            // Hämta alla användare som har rollen "Stylist"
            var stylists = await _userManager.GetUsersInRoleAsync("Stylist");

            // Skapa en SelectList för dropdownen med alla stylisters namn och ID
            ViewData["StylistId"] = new SelectList(stylists, "Id", "UserName", selectedStylistId);

            // Skapa en query för scheman, börja med att inkludera Stylist för relationen
            IQueryable<StylistSchedule> schedulesQuery = _context.StylistSchedule
                .Include(s => s.Stylist)
                .OrderByDescending(s => s.Day); // Nyast först

            // Filtrera om en stylist är vald
            if (!string.IsNullOrEmpty(selectedStylistId))
            {
                schedulesQuery = schedulesQuery.Where(s => s.StylistId == selectedStylistId);
            }

            // Hämta alla scheman (eller filtrerade baserat på stylist-id)
            var schedules = await schedulesQuery.ToListAsync();

            // Hämta alla bokningar som matchar någon av schemats stylist och dag
            var bookingDates = schedules.Select(s => s.Day.Date).ToList();
            var stylistIds = schedules.Select(s => s.StylistId).Distinct().ToList();

            var bookings = await _context.Bookings
                .Where(b => stylistIds.Contains(b.StylistId) && bookingDates.Contains(b.BookingTime.Date))
                .ToListAsync();

            // SKapa view model
            var viewModels = schedules.Select(s => new StylistScheduleWithBookingCountViewModel
            {
                Id = s.Id,
                StylistName = s.Stylist.Firstname,
                StylistId = s.StylistId,
                Day = s.Day,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                BreakStart = s.BreakStart,
                BreakEnd = s.BreakEnd,
                BufferTime = s.BufferTime,
                IsAvailable = s.IsAvailable,
                BookedCount = bookings.Count(b => b.StylistId == s.StylistId && b.BookingTime.Date == s.Day.Date)
            }).ToList();

            return View(viewModels);
        }

        // GET: StylistSchedule/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stylistSchedule = await _context.StylistSchedule
                .Include(s => s.Stylist)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stylistSchedule == null)
            {
                return NotFound();
            }

            return View(stylistSchedule);
        }

        // GET: StylistSchedule/Create
        public async Task<IActionResult> Create()
        {
            if (User.IsInRole("Admin"))
            {
                // Hämta alla användare med rollen "Stylist"
                var stylists = await _userManager.GetUsersInRoleAsync("Stylist");

                // Skapa en SelectList med dessa stylister
                ViewData["StylistId"] = new SelectList(stylists, "Id", "UserName");
            }
            else
            {
                // Om det är en stylist, sätt StylistId till den inloggade användaren (kan inte välja andra stylister)
                var currentUser = await _userManager.GetUserAsync(User);
                ViewData["StylistId"] = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "UserName");
            }

            return View();
        }

        // POST: StylistSchedule/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StylistId,Day,StartTime,EndTime,BreakStart,BreakEnd,IsAvailable,BufferTime")] StylistSchedule stylistSchedule)
        {

            // Om användaren inte är en admin, sätt StylistId till den inloggade användaren
            if (!User.IsInRole("Admin"))
            {
                stylistSchedule.StylistId = _userManager.GetUserId(User); // Sätt StylistId till den inloggade användaren
            }

            var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

            if (ModelState.IsValid)
            {
                _context.Add(stylistSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }


            if (User.IsInRole("Admin"))
            {
                // Hämta alla användare som har rollen "Stylist"
                var stylists = await _userManager.GetUsersInRoleAsync("Stylist");

                // Skapa en SelectList med stylisternas Id och UserName
                ViewData["StylistId"] = new SelectList(stylists, "Id", "UserName", stylistSchedule.StylistId);
            }
            else
            {
                // Om det inte är en admin, sätt dropdownen till den inloggade användaren
                var currentUser = await _userManager.GetUserAsync(User);
                ViewData["StylistId"] = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "UserName", currentUser.Id);
            }

            return View(stylistSchedule);
        }

        // GET: StylistSchedule/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stylistSchedule = await _context.StylistSchedule.FindAsync(id);
            if (stylistSchedule == null)
            {
                return NotFound();
            }

            // Läs in inloggad användare
            var currentUser = await _userManager.GetUserAsync(User);
            // Är användaren inte Admin eller ägare av schemat
            if (!User.IsInRole("Admin") && stylistSchedule.StylistId != currentUser.Id)
            {
                return Forbid();
            }

            var stylist = await _userManager.FindByIdAsync(stylistSchedule.StylistId);
            if (stylist != null) ViewData["StylistName"] = $"{stylist.Firstname} {stylist.Lastname}";

            ViewData["StylistId"] = new SelectList(_context.Users, "Id", "Id", stylistSchedule.StylistId);
            return View(stylistSchedule);
        }

        // POST: StylistSchedule/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StylistId,Day,StartTime,EndTime,BreakStart,BreakEnd,IsAvailable,BufferTime")] StylistSchedule stylistSchedule)
        {
            if (id != stylistSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kontrollera att användaren är Admin eller ägaren av schemat
                    var currentUser = await _userManager.GetUserAsync(User);
                    if (!User.IsInRole("Admin") && stylistSchedule.StylistId != currentUser.Id)
                    {
                        return Forbid(); // Eller en annan lämplig hantering av åtkomst nekad
                    }

                    var existingSchedule = await _context.StylistSchedule.FindAsync(id);
                    if (existingSchedule == null)
                    {
                        return NotFound();
                    }

                    existingSchedule.StartTime = stylistSchedule.StartTime;
                    existingSchedule.EndTime = stylistSchedule.EndTime;
                    existingSchedule.BreakStart = stylistSchedule.BreakStart;
                    existingSchedule.BreakEnd = stylistSchedule.BreakEnd;

                    _context.Update(existingSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StylistScheduleExists(stylistSchedule.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StylistId"] = new SelectList(_context.Users, "Id", "Id", stylistSchedule.StylistId);
            return View(stylistSchedule);
        }

        // GET: StylistSchedule/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var stylistSchedule = await _context.StylistSchedule
                .Include(s => s.Stylist)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (stylistSchedule == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && currentUser.Id != stylistSchedule.StylistId)
            {
                return Forbid();
            }

            return View(stylistSchedule);
        }

        // POST: StylistSchedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var stylistSchedule = await _context.StylistSchedule.FindAsync(id);
            if (stylistSchedule != null)
            {
                _context.StylistSchedule.Remove(stylistSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StylistScheduleExists(int id)
        {
            return _context.StylistSchedule.Any(e => e.Id == id);
        }
    }
}
