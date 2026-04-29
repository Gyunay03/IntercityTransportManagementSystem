using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using Route = IntercityTransportManagementSystem.Models.Route;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.ViewModels;

namespace IntercityTransportManagementSystem.Controllers
{
    public class RoutesController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public RoutesController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Routes
        public async Task<IActionResult> Index(string searchString, string sortOrder, int page = 1, int pageSize = 5)
        {
            var routesQuery = _context.Routes
                .AsNoTracking()
                .AsQueryable();

            // Търсене по начална и/или крайна дестинация
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                routesQuery = routesQuery.Where(r =>
                    r.StartDestination.Contains(searchString) ||
                    r.FinalDestination.Contains(searchString));
            }

            // Сортиране
            switch (sortOrder)
            {
                case "startDestination":
                    routesQuery = routesQuery.OrderBy(r => r.StartDestination);
                    break;
                case "startDestination_descending":
                    routesQuery = routesQuery.OrderByDescending(r => r.StartDestination);
                    break;
                case "finalDestination":
                    routesQuery = routesQuery.OrderBy(r => r.FinalDestination);
                    break;
                case "finalDestination_descending":
                    routesQuery = routesQuery.OrderByDescending(r => r.FinalDestination);
                    break;
                case "distance":
                    routesQuery = routesQuery.OrderBy(r => r.Distance);
                    break;
                case "distance_descending":
                    routesQuery = routesQuery.OrderByDescending(r => r.Distance);
                    break;
                case "estimatedDuration":
                    routesQuery = routesQuery.OrderBy(r => r.EstimatedDuration);
                    break;
                case "estimatedDuration_descending":
                    routesQuery = routesQuery.OrderByDescending(r => r.EstimatedDuration);
                    break;
                case "ticketPrice":
                    routesQuery = routesQuery.OrderBy(r => r.TicketPrice);
                    break;
                case "ticketPrice_descending":
                    routesQuery = routesQuery.OrderByDescending(r => r.TicketPrice);
                    break;
                default:
                    routesQuery = routesQuery.OrderBy(r => r.StartDestination);
                    break;
            }

            // Странициране
            var routes = await routesQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalRoutes = await routesQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRoutes / (double)pageSize);

            var viewModel = new RouteIndexViewModel
            {
                Routes = routes,
                SearchString = searchString,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Routes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // GET: Routes/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Routes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Create([Bind("StartDestination,FinalDestination, Distance, EstimatedDuration, TicketPrice")] Route route)
        {
            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(route);
        }

        // GET: Routes/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                return NotFound();
            }
            return View(route);
        }

        // POST: Routes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,StartDestination,FinalDestination, Distance, EstimatedDuration, TicketPrice")] Route route)
        {
            if (id != route.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingRoute = await _context.Routes.FindAsync(id);
                    if (existingRoute == null)
                    {
                        return NotFound();
                    }

                    existingRoute.StartDestination = route.StartDestination;
                    existingRoute.FinalDestination = route.FinalDestination;
                    existingRoute.Distance = route.Distance;
                    existingRoute.EstimatedDuration = route.EstimatedDuration;
                    existingRoute.TicketPrice = route.TicketPrice;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.Id))
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
            return View(route);
        }

        // GET: Routes/Delete/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // POST: Routes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                _context.Routes.Remove(route);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteExists(int id)
        {
            return _context.Routes.Any(e => e.Id == id);
        }
    }
}
