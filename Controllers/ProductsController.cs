using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Data;
using WholesaleHub.Models;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Warehouse,Customer")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Products.Include(p => p.Inventory).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search) || p.Category.Contains(search));
            }

            ViewData["Search"] = search;
            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin,Warehouse")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var inv = new Inventory { ProductID = product.ProductID, QuantityOnHand = 0, ReorderLevel = 10 };
            _context.Inventories.Add(inv);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product created successfully!";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid) return View(product);

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductID == id);
                if (inv != null) _context.Inventories.Remove(inv);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}