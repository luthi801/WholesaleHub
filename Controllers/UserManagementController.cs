using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Claims;
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

        public async Task<IActionResult> Index(string? search, string? role, bool archived = false)
        {
                var users = await _context.Users.ToListAsync();
                var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                    ? parsedUserId
                    : 0;
                var filteredUsers = users
                    .Where(user => (ManagedRoles.Contains(user.Role) || (user.UserID == currentUserId && user.Role == "Admin")) && user.IsArchived == archived);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    filteredUsers = filteredUsers.Where(user =>
                        user.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        user.UserName.Contains(search, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrWhiteSpace(role) && ManagedRoles.Contains(role))
                    filteredUsers = filteredUsers.Where(user => user.Role == role);

            ViewBag.Search = search;
            ViewBag.Role = role;
            ViewBag.Archived = archived;
                return View(filteredUsers.OrderBy(user => user.Role).ThenBy(user => user.Name).ToList());
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

            IDbContextTransaction? transaction = _context.Database.IsRelational()
                ? await _context.Database.BeginTransactionAsync()
                : null;
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

            if (transaction != null)
                await transaction.CommitAsync();
            TempData["Success"] = $"{model.Role} account created and is ready to use.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await FindEditableUser(id);
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
            var user = await FindEditableUser(model.UserID);
            if (user == null) return NotFound();

            var isOwnAdminProfile = user.Role == "Admin";
            if (isOwnAdminProfile)
            {
                model.Role = "Admin";
            }
            else if (model.Role == "Admin")
            {
                ModelState.AddModelError(nameof(model.Role), "Only the signed-in administrator can keep an Admin profile.");
            }

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

        private async Task<User?> FindEditableUser(int userId)
        {
            var currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var parsedUserId)
                ? parsedUserId
                : 0;

            return await _context.Users.FirstOrDefaultAsync(user =>
                user.UserID == userId &&
                (ManagedRoles.Contains(user.Role) || (user.Role == "Admin" && user.UserID == currentUserId)));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserID == id && ManagedRoles.Contains(item.Role));
            if (user == null) return NotFound();

            user.IsArchived = true;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{user.Name}'s account was archived. Their business history has been kept.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserID == id && ManagedRoles.Contains(item.Role));
            if (user == null) return NotFound();

            user.IsArchived = false;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{user.Name}'s account was restored.";
            return RedirectToAction(nameof(Index), new { archived = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(item => item.UserID == id && ManagedRoles.Contains(item.Role));
            if (user == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(item => item.UserID == id);
            var hasUserActivity = await _context.AuditLogs.AnyAsync(item => item.UserID == id)
                || await _context.InventoryTransactions.AnyAsync(item => item.UserID == id)
                || await _context.PurchaseOrders.AnyAsync(item => item.UserID == id)
                || await _context.Deliveries.AnyAsync(item => item.UserID == id)
                || await _context.CustomerPayments.AnyAsync(item => item.UserID == id)
                || await _context.CustomerCrmInteractions.AnyAsync(item => item.UserID == id);

            var hasCustomerActivity = customer != null && (
                await _context.SalesOrders.AnyAsync(item => item.CustomerID == customer.CustomerID)
                || await _context.AccountsReceivables.AnyAsync(item => item.CustomerID == customer.CustomerID)
                || await _context.CustomerCrmInteractions.AnyAsync(item => item.CustomerID == customer.CustomerID));

            if (hasUserActivity || hasCustomerActivity)
            {
                TempData["Error"] = "This account has business history and cannot be permanently deleted. Archive it to preserve its records.";
                return RedirectToAction(nameof(Index), new { archived = user.IsArchived });
            }

            if (customer != null) _context.Customers.Remove(customer);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{user.Name}'s account was permanently deleted.";
            return RedirectToAction(nameof(Index), new { archived = user.IsArchived });
        }
    }
}
