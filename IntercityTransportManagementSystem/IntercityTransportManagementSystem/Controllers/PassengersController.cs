using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.ViewModels;

namespace IntercityTransportManagementSystem.Controllers
{
    public class PassengersController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public PassengersController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Passengers
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1, int pageSize = 5)
        {
            var passengersQuery = _context.Passengers
                .AsNoTracking()
                .AsQueryable();

            // Търсене по име, фамилия и/или имейл адрес
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                passengersQuery = passengersQuery.Where(p =>
                    p.Name.Contains(searchString) ||
                    p.LastName.Contains(searchString) ||
                    p.Email.Contains(searchString));
            }

            // Сортиране
            switch (sortOrder)
            {
                case "name":
                    passengersQuery = passengersQuery.OrderBy(p => p.Name);
                    break;
                case "name_descending":
                    passengersQuery = passengersQuery.OrderByDescending(p => p.Name);
                    break;
                case "lastName":
                    passengersQuery = passengersQuery.OrderBy(p => p.LastName);
                    break;
                case "lastName_descending":
                    passengersQuery = passengersQuery.OrderByDescending(p => p.LastName);
                    break;
                case "email":
                    passengersQuery = passengersQuery.OrderBy(p => p.Email);
                    break;
                case "email_descending":
                    passengersQuery = passengersQuery.OrderByDescending(p => p.Email);
                    break;
                default:
                    passengersQuery = passengersQuery.OrderBy(p => p.Name);
                    break;
            }

            // Странициране
            var passengers = await passengersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPassengers = await passengersQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalPassengers / (double)pageSize);

            var viewModel = new PassengerIndexViewModel
            {
                Passengers = passengers,
                SearchString = searchString,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Passengers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (passenger == null)
            {
                return NotFound();
            }

            return View(passenger);
        }

        // GET: Passengers/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Passengers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Name,LastName,Email")] Passenger passenger)
        {
            if (ModelState.IsValid)
            {
                passenger = new Passenger
                {
                    Name = passenger.Name,
                    LastName = passenger.LastName,
                    Email = passenger.Email,
                    UserId = passenger.UserId
                };

                _context.Add(passenger);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(passenger);
        }

        // GET: Passengers/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passenger = await _context.Passengers.FindAsync(id);
            if (passenger == null)
            {
                return NotFound();
            }
            return View(passenger);
        }

        // POST: Passengers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,Email")] Passenger passenger)
        {
            if (id != passenger.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingPassenger = await _context.Passengers.FindAsync(id);
                    if (existingPassenger == null)
                    {
                        return NotFound();
                    }

                    existingPassenger.Name = passenger.Name;
                    existingPassenger.LastName = passenger.LastName;
                    existingPassenger.Email = passenger.Email;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PassengerExists(passenger.Id))
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
            return View(passenger);
        }

        // GET: Passengers/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var passenger = await _context.Passengers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (passenger == null)
            {
                return NotFound();
            }

            return View(passenger);
        }

        // POST: Passengers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var passenger = await _context.Passengers.FindAsync(id);
            if (passenger != null)
            {
                _context.Passengers.Remove(passenger);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PassengerExists(int id)
        {
            return _context.Passengers.Any(e => e.Id == id);
        }
    }
}
