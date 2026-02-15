using IntercityTransportManagementSystem.Enums;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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

                var token = Guid.NewGuid().ToString();
                user.EmailVerificationToken = token;
                user.EmailVerificationTokenExpiration = DateTime.UtcNow.AddHours(24);
                user.IsEmailVerified = false;

                // Присвояване на хешираната парола
                user.Password = passwordHasher.HashPassword(user, model.Password);

                // Запазване на потребителя в базата данни
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var verificationLink = Url.Action(
                    "VerifyEmail", "Account",
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
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Email == model.Email);

                if (user != null && VerifyUserPassword(user, model.Password))
                {
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

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Невалидни данни за вход.");
            }

            return View(model);
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

            var user = await _context.Users.FirstOrDefaultAsync(u => 
                u.Email == model.Email && 
                u.PasswordResetToken == model.Token &&
                u.PasswordResetTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                ModelState.AddModelError("", "Невалиден или изтекъл линк за смяна на паролата.");
                return View(model);
            }

            var passwordHasher = new PasswordHasher<User>();
            user.Password = passwordHasher.HashPassword(user, model.NewPassword);

            user.PasswordResetToken = null;
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

            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.EmailVerificationToken == token &&
                u.EmailVerificationTokenExpiration > DateTime.UtcNow);

            if (user == null)
            {
                return View("VerificationFailed");
            }

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
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

            var token = Guid.NewGuid().ToString();
            
            user.PasswordResetToken = token;
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
    }
}