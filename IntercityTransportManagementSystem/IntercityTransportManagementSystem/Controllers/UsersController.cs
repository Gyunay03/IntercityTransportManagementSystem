using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.ViewModels;

namespace IntercityTransportManagementSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public UsersController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index(string searchString, string roleFilter, string sortOrder, bool? isActive, int page = 1, int pageSize = 5)
        {
            var usersQuery = _context.Users
                .AsNoTracking()
                .AsQueryable();

            // Търсене по име, фамилия и/или имейл адрес
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                usersQuery = usersQuery.Where(u =>
                    u.Name.Contains(searchString) ||
                    u.LastName.Contains(searchString) ||
                    u.Email.Contains(searchString));
            }

            // Филтриране по роля
            if (!string.IsNullOrEmpty(roleFilter) &&
                Enum.TryParse<UserRole>(roleFilter, out var parsedRole))
            {
                usersQuery = usersQuery.Where(u => u.Role == parsedRole);
            }

            // Филтриране по статус
            if (isActive.HasValue)
            {
                usersQuery = usersQuery.Where(u => u.IsActive == isActive.Value);
            }

            var totalUsers = await usersQuery.CountAsync();

            // Сортиране
            switch(sortOrder)
            {
                case "name":
                    usersQuery = usersQuery.OrderBy(u => u.Name);
                    break;
                case "name_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.Name);
                    break;
                case "lastName":
                    usersQuery = usersQuery.OrderBy(u => u.LastName);
                    break;
                case "lastName_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.LastName);
                    break;
                case "email":
                    usersQuery = usersQuery.OrderBy(u => u.Email);
                    break;
                case "email_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.Email);
                    break;
                case "role":
                    usersQuery = usersQuery.OrderBy(u => u.Role);
                    break;
                case "role_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.Role);
                    break;
                case "isActive":
                    usersQuery = usersQuery.OrderBy(u => u.IsActive);
                    break;
                case "isActive_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.IsActive);
                    break;
                case "isEmailVerified":
                    usersQuery = usersQuery.OrderBy(u => u.IsEmailVerified);
                    break;
                case "isEmailVerified_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.IsEmailVerified);
                    break;
                case "createdAt":
                    usersQuery = usersQuery.OrderBy(u => u.CreatedAt);
                    break;
                case "createdAt_descending":
                    usersQuery = usersQuery.OrderByDescending(u => u.CreatedAt);
                    break;
                default:
                    usersQuery = usersQuery.OrderBy(u => u.Name);
                    break;
            };

            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var viewModel = new UserIndexViewModel
            {
                Users = users,
                SearchString = searchString,
                RoleFilter = roleFilter,
                IsActive = isActive,
                SortOrder = sortOrder,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        [Authorize(Roles = "Administrator")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,LastName,Email,Password,Role,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(user.Password))
                {
                    var passwordHasher = new PasswordHasher<User>();
                    user.Password = passwordHasher.HashPassword(user, user.Password);
                }

                user.CreatedAt = DateTime.UtcNow;

                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditUserViewModel
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive,
                IsEmailVerified = user.IsEmailVerified
            };

            return View(viewModel);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine(error.ErrorMessage);
                }
                return View(model);
            }    
                

            var user = await _context.Users.FindAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.Name = model.Name;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.Role = model.Role;
            user.IsActive = model.IsActive;
            user.IsEmailVerified = model.IsEmailVerified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
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

        // GET: Users/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Метод за активиране на потребителски профил
        public async Task<IActionResult> UnlockAccount(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Метод за потвърждаване на имейл адрес
        public async Task<IActionResult> VerifyEmail(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsEmailVerified = true;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Метод за нулиране на парола
        public async Task<IActionResult> ResetPassword(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Генериране на случайна временна парола
            string temporaryPassword = GenerateTemporaryPassword();

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, temporaryPassword);

            // Нулиране на заключването и броя на неуспешните опити за вход
            user.LockoutEnd = null;
            user.FailedLoginAttempts = 0;

            TempData["ResetPasswordMessage"] = $"Временната парола за {user.Email} е: {temporaryPassword}";

            _context.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id });
        }

        // Метод за генериране на силна временна парола
        private string GenerateTemporaryPassword(int length = 12)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?";
            var random = new Random();
            return new string(Enumerable.Repeat(validChars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}