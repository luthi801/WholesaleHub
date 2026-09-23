using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.Models;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Accountant,Warehouse")]
    public class CrmController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CrmController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Customers = await _context.Customers.ToListAsync();
            var interactions = await _context.CustomerCrmInteractions
                .Include(c => c.Customer)
                .Include(c => c.User)
                .OrderByDescending(c => c.InteractionDate)
                .ToListAsync();
            return View(interactions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerCrmInteraction model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid interaction data.";
                return RedirectToAction(nameof(Index));
            }

            model.UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            model.InteractionDate = DateTime.Now;

            _context.CustomerCrmInteractions.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Interaction recorded!";
            return RedirectToAction(nameof(Index));
        }
    }
}