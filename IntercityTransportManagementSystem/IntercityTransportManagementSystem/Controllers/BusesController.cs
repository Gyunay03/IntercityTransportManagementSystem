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
    public class BusesController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public BusesController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Buses
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1, int pageSize = 5)
        {
            var busesQuery = _context.Buses
                .AsNoTracking()
                .AsQueryable();

            // Търсене по регистрационен номер
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                busesQuery = busesQuery.Where(b =>
                    b.RegistrationNumber.Contains(searchString));
            }

            // Сортиране
            switch (sortOrder)
            {
                case "registrationNumber":
                    busesQuery = busesQuery.OrderBy(b => b.RegistrationNumber);
                    break;
                case "registrationNumber_descending":
                    busesQuery = busesQuery.OrderByDescending(b => b.RegistrationNumber);
                    break;
                case "capacity":
                    busesQuery = busesQuery.OrderBy(b => b.Capacity);
                    break;
                case "capacity_descending":
                    busesQuery = busesQuery.OrderByDescending(b => b.Capacity);
                    break;
                default:
                    busesQuery = busesQuery.OrderBy(b => b.RegistrationNumber);
                    break;
            }

            // Странициране
            var buses = await busesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalBuses = await busesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalBuses / (double)pageSize);

            var viewModel = new BusIndexViewModel
            {
                Buses = buses,
                SearchString = searchString,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };
            return View(viewModel);
        }

        // GET: Buses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Buses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        // GET: Buses/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Buses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,RegistrationNumber,Capacity")] Bus bus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(bus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bus);
        }

        // GET: Buses/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Buses.FindAsync(id);
            if (bus == null)
            {
                return NotFound();
            }
            return View(bus);
        }

        // POST: Buses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,RegistrationNumber,Capacity")] Bus bus)
        {
            if (id != bus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusExists(bus.Id))
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
            return View(bus);
        }

        // GET: Buses/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Buses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bus == null)
            {
                return NotFound();
            }

            return View(bus);
        }

        // POST: Buses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var bus = await _context.Buses.FindAsync(id);
            if (bus != null)
            {
                _context.Buses.Remove(bus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusExists(int id)
        {
            return _context.Buses.Any(e => e.Id == id);
        }
    }
}