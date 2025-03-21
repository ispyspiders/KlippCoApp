using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KlippCoApp.Data;
using KlippCoApp.Models;

namespace KlippCoApp.Controllers
{
    public class StylistScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StylistScheduleController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Create()
        {
            ViewData["StylistId"] = new SelectList(_context.Users, "Id", "Id");
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
                _context.Add(stylistSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StylistId"] = new SelectList(_context.Users, "Id", "Id", stylistSchedule.StylistId);
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
