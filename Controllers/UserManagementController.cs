using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private static readonly string[] ManagedRoles = { "Accountant", "Warehouse", "Customer" };
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();

        public UserManagementController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? role)
        {
            var query = _context.Users
                .Where(user => ManagedRoles.Contains(user.Role))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(user => user.Name.Contains(search) || user.UserName.Contains(search));

            if (!string.IsNullOrWhiteSpace(role) && ManagedRoles.Contains(role))
                query = query.Where(user => user.Role == role);

            ViewBag.Search = search;
            ViewBag.Role = role;
            return View(await query.OrderBy(user => user.Role).ThenBy(user => user.Name).ToListAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AdminUserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminUserCreateViewModel model)
        {
            model.UserName = model.UserName.Trim();
            if (await _context.Users.AnyAsync(user => user.UserName == model.UserName))
                ModelState.AddModelError(nameof(model.UserName), "That username is already in use.");

            if (model.Role == "Customer")
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName)) ModelState.AddModelError(nameof(model.CompanyName), "Company name is required for customers.");
                if (string.IsNullOrWhiteSpace(model.ContactPerson)) ModelState.AddModelError(nameof(model.ContactPerson), "Contact person is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Email)) ModelState.AddModelError(nameof(model.Email), "Email is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Phone)) ModelState.AddModelError(nameof(model.Phone), "Phone is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Address)) ModelState.AddModelError(nameof(model.Address), "Address is required for customers.");
            }

            if (!ModelState.IsValid) return View(model);

            await using var transaction = await _context.Database.BeginTransactionAsync();
            var user = new User
            {
                Name = model.Name.Trim(),
                UserName = model.UserName,
                Role = model.Role
            };
            user.Password = _passwordHasher.HashPassword(user, model.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (model.Role == "Customer")
            {
                _context.Customers.Add(new Customer
                {
                    UserID = user.UserID,
                    CompanyName = model.CompanyName.Trim(),
                    ContactPerson = model.ContactPerson.Trim(),
                    Email = model.Email.Trim(),
                    Phone = model.Phone.Trim(),
                    Address = model.Address.Trim()
                });
                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
            TempData["Success"] = $"{model.Role} account created and is ready to use.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserID == id && ManagedRoles.Contains(item.Role));
            if (user == null) return NotFound();

            var customer = user.Role == "Customer"
                ? await _context.Customers.FirstOrDefaultAsync(item => item.UserID == user.UserID)
                : null;

            return View(new AdminUserEditViewModel
            {
                UserID = user.UserID,
                Name = user.Name,
                UserName = user.UserName,
                Role = user.Role,
                CompanyName = customer?.CompanyName ?? string.Empty,
                ContactPerson = customer?.ContactPerson ?? string.Empty,
                Email = customer?.Email ?? string.Empty,
                Phone = customer?.Phone ?? string.Empty,
                Address = customer?.Address ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AdminUserEditViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserID == model.UserID && ManagedRoles.Contains(item.Role));
            if (user == null) return NotFound();

            model.UserName = model.UserName.Trim();
            if (await _context.Users.AnyAsync(item => item.UserName == model.UserName && item.UserID != model.UserID))
                ModelState.AddModelError(nameof(model.UserName), "That username is already in use.");

            if (model.Role == "Customer")
            {
                if (string.IsNullOrWhiteSpace(model.CompanyName)) ModelState.AddModelError(nameof(model.CompanyName), "Company name is required for customers.");
                if (string.IsNullOrWhiteSpace(model.ContactPerson)) ModelState.AddModelError(nameof(model.ContactPerson), "Contact person is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Email)) ModelState.AddModelError(nameof(model.Email), "Email is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Phone)) ModelState.AddModelError(nameof(model.Phone), "Phone is required for customers.");
                if (string.IsNullOrWhiteSpace(model.Address)) ModelState.AddModelError(nameof(model.Address), "Address is required for customers.");
            }

            if (!ModelState.IsValid) return View(model);

            user.Name = model.Name.Trim();
            user.UserName = model.UserName;
            user.Role = model.Role;
            if (!string.IsNullOrWhiteSpace(model.Password)) user.Password = _passwordHasher.HashPassword(user, model.Password);

            var customer = await _context.Customers.FirstOrDefaultAsync(item => item.UserID == user.UserID);
            if (model.Role == "Customer")
            {
                customer ??= new Customer { UserID = user.UserID };
                customer.CompanyName = model.CompanyName.Trim();
                customer.ContactPerson = model.ContactPerson.Trim();
                customer.Email = model.Email.Trim();
                customer.Phone = model.Phone.Trim();
                customer.Address = model.Address.Trim();
                if (customer.CustomerID == 0) _context.Customers.Add(customer);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Account updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}