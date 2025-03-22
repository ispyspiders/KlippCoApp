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

namespace KlippCoApp.Controllers
{
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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.StylistSchedule.Include(s => s.Stylist);
            return View(await applicationDbContext.ToListAsync());
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
public async Task<IActionResult> Create([Bind("Id,StylistId,Day,StartTime,EndTime,BreakStart,BreakEnd,IsAvailable,BufferTime")] StylistSchedule stylistSchedule)
{
    if (ModelState.IsValid)
    {
        // Om användaren inte är en admin, sätt StylistId till den inloggade användaren
        if (!User.IsInRole("Admin"))
        {
            stylistSchedule.StylistId = _userManager.GetUserId(User); // Sätt StylistId till den inloggade användaren
        }

        _context.Add(stylistSchedule);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Om något gick fel, visa listan för att skapa en stylist
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
        ViewData["StylistId"] = new SelectList(new List<ApplicationUser> { currentUser }, "Id", "UserName", stylistSchedule.StylistId);
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
                    _context.Update(stylistSchedule);
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
