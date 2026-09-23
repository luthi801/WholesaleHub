using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.Models;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Accountant,Customer")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Customers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.CompanyName.Contains(search) || c.ContactPerson.Contains(search));
            }
            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin,Accountant")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Create(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Customer created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Edit(Customer customer)
        {
            if (!ModelState.IsValid) return View(customer);

            _context.Customers.Update(customer);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Customer details updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Delete(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Customer record deleted.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyProfile()
        {
            var customer = await GetCurrentCustomer();
            return customer == null ? NotFound() : View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyProfile(Customer profile)
        {
            var customer = await GetCurrentCustomer();
            if (customer == null) return NotFound();
            if (!ModelState.IsValid) return View(profile);

            customer.CompanyName = profile.CompanyName;
            customer.ContactPerson = profile.ContactPerson;
            customer.Email = profile.Email;
            customer.Phone = profile.Phone;
            customer.Address = profile.Address;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Your personal information was updated.";
            return RedirectToAction(nameof(MyProfile));
        }

        private async Task<Customer?> GetCurrentCustomer()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(customer => customer.UserID == userId);
        }
    }
}