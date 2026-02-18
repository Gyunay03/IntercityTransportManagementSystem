using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IntercityTransportManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IntercityTransportManagementSystemDatabaseContext _context;

        public AccountController(IntercityTransportManagementSystemDatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Този имейл адрес вече е регистриран.");
                    return View(model);
                }

                // Хеширане на паролата
                PasswordHasher<User> passwordHasher = new PasswordHasher<User>();

                // Създаване на нов потребител
                var user = new User
                {
                    Name = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Role = UserRole.Passenger,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var tokenBytes = RandomNumberGenerator.GetBytes(64);
                var token = WebEncoders.Base64UrlEncode(tokenBytes);
                var tokenHash = ComputeSha256Hash(token);

                user.EmailVerificationTokenHash = tokenHash;
                user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24);
                user.IsEmailVerified = false;

                // Присвояване на хешираната парола
                user.Password = passwordHasher.HashPassword(user, model.Password);

                // Запазване на потребителя в базата данни
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var verificationLink = Url.Action("VerifyEmail", "Account", 
                    new { token = token }, Request.Scheme);

                TempData["VerificationLink"] = verificationLink;

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Невалиден имейл или парола.");
                return View(model);
            }

            // Проверка за временно заключен акаунт
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError("", "Акаунтът е временно заключен.");
                return View(model);
            }
            
            // Проверка на паролата
            bool passwordValid = VerifyUserPassword(user, model.Password);

            if (!passwordValid)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                }

                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Невалиден имейл или парола.");
                return View(model);
            }

            // Успешно влизане - нулиране на брояча за неуспешни опити и премахване на заключването
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;

            // Проверка дали имейлът е потвърден
            if (!user.IsEmailVerified)
            {
                ModelState.AddModelError("", "Моля, потвърдете имейл адреса си.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Акаунтът е деактивиран.");
                return View(model);
            }

            await _context.SaveChangesAsync();

            // Създаване на claims за автентикация
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name +  " " + user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity), authProperties);
                
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Липсва токен за смяна на паролата.");
            }

            return View(new ResetPasswordViewModel
            {
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var tokenHash = ComputeSha256Hash(model.Token);

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == model.Email && 
                u.PasswordResetTokenHash == tokenHash &&
                u.PasswordResetTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Невалиден или изтекъл линк за смяна на паролата.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, model.NewPassword);

            user.PasswordResetTokenHash = null;
            user.PasswordResetTokenExpiration = null;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Паролата е променена успешно";
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Линкът е невалиден.");
            }

            var tokenHash = ComputeSha256Hash(token);

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.EmailVerificationTokenHash == tokenHash &&
                u.EmailVerificationTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                return View("VerificationFailed");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationTokenHash = null;
            user.EmailVerificationTokenExpiration = null;

            await _context.SaveChangesAsync();

            return View("VerificationSuccess");
        }

        private bool VerifyUserPassword(User user, string enteredPassword)
        {
            // Проверка на паролата(чрез сравняване на хеш кода)
            PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.Password, enteredPassword);
            return result == PasswordVerificationResult.Success;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var tokenBytes = RandomNumberGenerator.GetBytes(64);
            var token = WebEncoders.Base64UrlEncode(tokenBytes);

            // Хеширане на токен
            var tokenHash = ComputeSha256Hash(token);

            user.PasswordResetTokenHash = tokenHash;
            user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var resetLink = Url.Action(
                "ResetPassword", "Account", 
                new { token = token}, Request.Scheme);

            TempData["ResetLink"] = resetLink;

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}