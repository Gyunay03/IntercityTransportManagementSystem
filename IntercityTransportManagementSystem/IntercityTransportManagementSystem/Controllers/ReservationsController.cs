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
    public class ReservationsController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public ReservationsController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var intercityTransportManagementSystemDatabaseContext = _context.Reservations.Include(r => r.Passenger).Include(r => r.Schedule).Include(r => r.Seat);
            return View(await intercityTransportManagementSystemDatabaseContext.ToListAsync());
        }

        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Schedule)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservations/Create
        public IActionResult Create()
        {
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id");
            ViewData["ScheduleId"] = new SelectList(_context.BusSchedules, "Id", "Id");
            ViewData["SeatId"] = new SelectList(_context.BusSeats, "Id", "Id");
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PassengerId,ScheduleId,SeatId,Status,ReservationTime")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id", reservation.PassengerId);
            ViewData["ScheduleId"] = new SelectList(_context.BusSchedules, "Id", "Id", reservation.ScheduleId);
            ViewData["SeatId"] = new SelectList(_context.BusSeats, "Id", "Id", reservation.SeatId);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id", reservation.PassengerId);
            ViewData["ScheduleId"] = new SelectList(_context.BusSchedules, "Id", "Id", reservation.ScheduleId);
            ViewData["SeatId"] = new SelectList(_context.BusSeats, "Id", "Id", reservation.SeatId);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PassengerId,ScheduleId,SeatId,Status,ReservationTime")] Reservation reservation)
        {
            if (id != reservation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reservation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReservationExists(reservation.Id))
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
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id", reservation.PassengerId);
            ViewData["ScheduleId"] = new SelectList(_context.BusSchedules, "Id", "Id", reservation.ScheduleId);
            ViewData["SeatId"] = new SelectList(_context.BusSeats, "Id", "Id", reservation.SeatId);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Passenger)
                .Include(r => r.Schedule)
                .Include(r => r.Seat)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                _context.Reservations.Remove(reservation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
