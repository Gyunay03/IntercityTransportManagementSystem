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
    public class DriversController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public DriversController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Drivers
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1, int pageSize = 5)
        {
            var driversQuery = _context.Drivers
                .AsNoTracking()
                .AsQueryable();

            // Търсене по име, фамилия, имейл адрес, телефонен номер и/или номер на шофьорска книжка
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                driversQuery = driversQuery.Where(d =>
                    d.Name.Contains(searchString) ||
                    d.LastName.Contains(searchString) ||
                    d.Email.Contains(searchString) ||
                    d.PhoneNumber.Contains(searchString) ||
                    d.LicenseNumber.Contains(searchString));
            }

            // Сортиране
            switch (sortOrder)
            {
                case "name":
                    driversQuery = driversQuery.OrderBy(d => d.Name);
                    break;
                case "name_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.Name);
                    break;
                case "lastName":
                    driversQuery = driversQuery.OrderBy(d => d.LastName);
                    break;
                case "lastName_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.LastName);
                    break;
                case "email":
                    driversQuery = driversQuery.OrderBy(d => d.Email);
                    break;
                case "email_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.Email);
                    break;
                case "phoneNumber":
                    driversQuery = driversQuery.OrderBy(d => d.PhoneNumber);
                    break;
                case "phoneNumber_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.PhoneNumber);
                    break;
                case "licenseNumber":
                    driversQuery = driversQuery.OrderBy(d => d.LicenseNumber);
                    break;
                case "licenseNumber_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.LicenseNumber);
                    break;
                case "hireDate":
                    driversQuery = driversQuery.OrderBy(d => d.HireDate);
                    break;
                case "hireDate_descending":
                    driversQuery = driversQuery.OrderByDescending(d => d.HireDate);
                    break;
                default:
                    driversQuery = driversQuery.OrderBy(d => d.Name);
                    break;
            }

            // Странициране
            var drivers = await driversQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalDrivers = await driversQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalDrivers / (double)pageSize);

            var viewModel = new DriverIndexViewModel
            {
                Drivers = drivers,
                SearchString = searchString,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };
            return View(viewModel);
        }

        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // GET: Drivers/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Drivers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("Name,LastName,Email,PhoneNumber,LicenseNumber,HireDate")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                _context.Add(driver);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(driver);
        }

        // GET: Drivers/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }
            return View(driver);
        }

        // POST: Drivers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,LastName,Email,PhoneNumber,LicenseNumber,HireDate")] Driver driver)
        {
            if (id != driver.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingDriver = await _context.Drivers.FindAsync(id);
                    if (existingDriver == null)
                    {
                        return NotFound();
                    }

                    existingDriver.Name = driver.Name;
                    existingDriver.LastName = driver.LastName;
                    existingDriver.Email = driver.Email;
                    existingDriver.PhoneNumber = driver.PhoneNumber;
                    existingDriver.LicenseNumber = driver.LicenseNumber;
                    existingDriver.HireDate = driver.HireDate;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.Id))
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
            return View(driver);
        }

        // GET: Drivers/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers.FindAsync(id);
            if (driver != null)
            {
                _context.Drivers.Remove(driver);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.Id == id);
        }
    }
}
