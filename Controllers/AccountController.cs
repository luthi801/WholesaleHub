using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.Security;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectForRole(User.FindFirstValue(ClaimTypes.Role));

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            var userName = model.UserName.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == userName);
            var passwordValid = user != null && _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password) != PasswordVerificationResult.Failed;
            if (user != null && !passwordValid && user.Password == model.Password)
            {
                user.Password = _passwordHasher.HashPassword(user, model.Password);
                passwordValid = true;
            }

            if (user == null || !passwordValid || user.IsArchived)
            {
                ModelState.AddModelError("", "Invalid login credentials.");
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
            }

            await SignInUser(user);

            _context.AuditLogs.Add(new AuditLog { UserID = user.UserID, ActionPerformed = $"User logged in: {user.UserName}" });
            await _context.SaveChangesAsync();

            return Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectForRole(user.Role);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public IActionResult Register(RegisterViewModel model)
        {
            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int uid))
            {
                _context.AuditLogs.Add(new AuditLog { UserID = uid, ActionPerformed = "User logged out" });
                await _context.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUser(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("UserName", user.UserName),
                new Claim(ApplicationSession.ClaimType, ApplicationSession.Id)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
        }

        private IActionResult RedirectForRole(string? role)
        {
            return role switch
            {
                "Warehouse" => RedirectToAction("Index", "Inventory"),
                "Accountant" => RedirectToAction("Index", "Dashboard"),
                "Customer" => RedirectToAction("Index", "Dashboard"),
                _ => RedirectToAction("Index", "Dashboard")
            };
        }
    }
}
