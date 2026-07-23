using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using IntercityTransportManagementSystem.ViewModels;

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
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index(string searchString, string sortOrder, string registrationNumberFilter, int page = 1, int pageSize = 20)
        {
            var busSeatsQuery = _context.BusSeats.
                Include(b => b.Bus)
                .OrderBy(b => b.Bus.RegistrationNumber)
                .ThenBy(b => b.Number)
                .AsNoTracking()
                .AsQueryable();

            // Търсене по регистрационен номер на автобус
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                busSeatsQuery = busSeatsQuery.Where(b =>
                    b.Bus.RegistrationNumber.Contains(searchString));
            }

            // Филтриране по регистрационен номер на автобус
            if (!string.IsNullOrEmpty(registrationNumberFilter))
            {
                busSeatsQuery = busSeatsQuery.Where(b => b.Bus.RegistrationNumber == registrationNumberFilter);
            }

            // Сортиране
            switch (sortOrder)
            {
                case "seatNumber":
                    busSeatsQuery = busSeatsQuery.OrderBy(b => b.Number);
                    break;

                case "seatNumber_descending":
                    busSeatsQuery = busSeatsQuery.OrderByDescending(b => b.Number);
                    break;

                case "registrationNumber":
                    busSeatsQuery = busSeatsQuery.OrderBy(b => b.Bus.RegistrationNumber);
                    break;
                
                case "registrationNumber_descending":
                    busSeatsQuery = busSeatsQuery.OrderByDescending(b => b.Bus.RegistrationNumber);
                    break;
                
                default:
                    busSeatsQuery = busSeatsQuery
                        .OrderBy(b => b.Bus.RegistrationNumber)
                        .ThenBy(b => b.Number);
                    break;
            }

            var registrationNumbers = await _context.Buses
                .Select(b => b.RegistrationNumber)
                .Distinct()
                .ToListAsync();

            // Странициране
            var busSeatsPage = await busSeatsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var allBusSeats = await busSeatsQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(allBusSeats / (double)pageSize);

            var viewModel = new BusSeatsIndexViewModel
            {
                BusSeats = busSeatsPage,
                SearchString = searchString,
                SortOrder = sortOrder,
                RegistrationNumberFilter = registrationNumberFilter,
                RegistrationNumbers = registrationNumbers,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: BusSeats/Details/5
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            FillDropdowns();
            return View();
        }

        // POST: BusSeats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Id,Number,BusId")] BusSeat busSeat)
        {
            if (ModelState.IsValid)
            {
                bool seatExists = _context.BusSeats
                    .Any(s => s.BusId == busSeat.BusId && s.Number == busSeat.Number);

                if (seatExists)
                {
                    ModelState.AddModelError("", "Това място вече е добавено (създадено) за този автобус.");
                }

                else
                {
                    _context.Add(busSeat);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                if (busSeat.Number < 1 || busSeat.Number > busSeat.Bus.Capacity)
                {
                    ModelState.AddModelError("", "Не може да се добави нулево или по-голямо място от капацитета на автобуса.");
                }
            }

            FillDropdowns(busSeat.BusId);
            return View(busSeat);
        }

        // GET: BusSeats/Edit/5
        [Authorize(Roles = "Administrator")]
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
            FillDropdowns(busSeat.BusId);
            return View(busSeat);
        }

        // POST: BusSeats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Number,BusId")] BusSeat busSeat)
        {
            if (id != busSeat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                bool seatExist = _context.BusSeats
                    .Any(s => s.BusId == busSeat.BusId && s.Number == busSeat.Number && s.Id != busSeat.Id);

                if (seatExist)
                {
                    ModelState.AddModelError("", "Това място вече съществува за този автобус.");
                }

                else if (busSeat.Number < 1 || busSeat.Number > busSeat.Bus.Capacity)
                {
                    ModelState.AddModelError("", "Не може да се добави нулево или по-голямо място от капацитета на автобуса.");
                }

                else
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
            }
            
            FillDropdowns(busSeat.BusId);
            return View(busSeat);
        }

        // GET: BusSeats/Delete/5
        [Authorize(Roles = "Administrator")]
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
        [Authorize(Roles = "Administrator")]
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

        private void FillDropdowns(int? selectedBusId = null)
        {
            ViewData["BusId"] = new SelectList(_context.Buses.AsNoTracking()
                .Select(b => new { b.Id, BusRegistrationNumber = b.RegistrationNumber }),
                "Id", "BusRegistrationNumber", selectedBusId);
        }

        [HttpGet]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSeats()
        {
            FillDropdowns();
            return View();
        }

        // Метод за генериране на места в автобус
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GenerateSeats(int busId)
        {
            var bus = await _context.Buses.FindAsync(busId);
            
            if (bus == null)
            {
                return NotFound();
            }

            bool seatsExists = await _context.BusSeats.AnyAsync(s => s.BusId == busId);

            if (seatsExists)
            {
                TempData["AlreadyGenerated"] = "Местата вече са генерирани за този автобус.";
                return RedirectToAction("Index", "BusSeats");
            }

            for (int i = 1; i <= bus.Capacity; i++)
            {
                var seat = new BusSeat
                {
                    Number = i,
                    BusId = busId
                };

                _context.BusSeats.Add(seat);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
