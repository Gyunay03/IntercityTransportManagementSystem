using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;

namespace IntercityTransportManagementSystem.Controllers
{
    public class BusSeatsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public BusSeatsController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: BusSeats
        public async Task<IActionResult> Index()
        {
            var intercityTransportManagementSystemDatabaseContext = _context.BusSeats.Include(b => b.Bus);
            return View(await intercityTransportManagementSystemDatabaseContext.ToListAsync());
        }

        // GET: BusSeats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSeat = await _context.BusSeats
                .Include(b => b.Bus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSeat == null)
            {
                return NotFound();
            }

            return View(busSeat);
        }

        // GET: BusSeats/Create
        public IActionResult Create()
        {
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id");
            return View();
        }

        // POST: BusSeats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Number,BusId")] BusSeat busSeat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(busSeat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSeat.BusId);
            return View(busSeat);
        }

        // GET: BusSeats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSeat = await _context.BusSeats.FindAsync(id);
            if (busSeat == null)
            {
                return NotFound();
            }
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSeat.BusId);
            return View(busSeat);
        }

        // POST: BusSeats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,BusId")] BusSeat busSeat)
        {
            if (id != busSeat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(busSeat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusSeatExists(busSeat.Id))
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
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSeat.BusId);
            return View(busSeat);
        }

        // GET: BusSeats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSeat = await _context.BusSeats
                .Include(b => b.Bus)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSeat == null)
            {
                return NotFound();
            }

            return View(busSeat);
        }

        // POST: BusSeats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var busSeat = await _context.BusSeats.FindAsync(id);
            if (busSeat != null)
            {
                _context.BusSeats.Remove(busSeat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusSeatExists(int id)
        {
            return _context.BusSeats.Any(e => e.Id == id);
        }
    }
}
