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
    public class BusSchedulesController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public BusSchedulesController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: BusSchedules
        public async Task<IActionResult> Index()
        {
            var intercityTransportManagementSystemDatabaseContext = _context.BusSchedules.Include(b => b.Bus).Include(b => b.Driver).Include(b => b.Route);
            return View(await intercityTransportManagementSystemDatabaseContext.ToListAsync());
        }

        // GET: BusSchedules/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules
                .Include(b => b.Bus)
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSchedule == null)
            {
                return NotFound();
            }

            return View(busSchedule);
        }

        // GET: BusSchedules/Create
        public IActionResult Create()
        {
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id");
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id");
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Id");
            return View();
        }

        // POST: BusSchedules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,RouteId,BusId,DriverId,DepartureTime,ArrivalTime")] BusSchedule busSchedule)
        {
            if (ModelState.IsValid)
            {
                _context.Add(busSchedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSchedule.BusId);
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", busSchedule.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Id", busSchedule.RouteId);
            return View(busSchedule);
        }

        // GET: BusSchedules/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules.FindAsync(id);
            if (busSchedule == null)
            {
                return NotFound();
            }
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSchedule.BusId);
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", busSchedule.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Id", busSchedule.RouteId);
            return View(busSchedule);
        }

        // POST: BusSchedules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RouteId,BusId,DriverId,DepartureTime,ArrivalTime")] BusSchedule busSchedule)
        {
            if (id != busSchedule.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(busSchedule);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusScheduleExists(busSchedule.Id))
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
            ViewData["BusId"] = new SelectList(_context.Buses, "Id", "Id", busSchedule.BusId);
            ViewData["DriverId"] = new SelectList(_context.Drivers, "Id", "Id", busSchedule.DriverId);
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Id", busSchedule.RouteId);
            return View(busSchedule);
        }

        // GET: BusSchedules/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var busSchedule = await _context.BusSchedules
                .Include(b => b.Bus)
                .Include(b => b.Driver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (busSchedule == null)
            {
                return NotFound();
            }

            return View(busSchedule);
        }

        // POST: BusSchedules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var busSchedule = await _context.BusSchedules.FindAsync(id);
            if (busSchedule != null)
            {
                _context.BusSchedules.Remove(busSchedule);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusScheduleExists(int id)
        {
            return _context.BusSchedules.Any(e => e.Id == id);
        }
    }
}
