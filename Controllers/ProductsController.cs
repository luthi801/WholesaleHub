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
        private readonly IWebHostEnvironment _environment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? search, string? category, string? stockStatus, string? sort, bool archived = false)
        {
            var query = _context.Products.Include(p => p.Inventory).Where(p => p.IsArchived == archived).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search) || p.Category.Contains(search) || p.Brand.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(p => p.Category == category);
            if (stockStatus == "In Stock") query = query.Where(p => p.Inventory != null && p.Inventory.Status == "In Stock");
            if (stockStatus == "Low Stock") query = query.Where(p => p.Inventory != null && p.Inventory.Status == "Low Stock");
            if (stockStatus == "Out of Stock") query = query.Where(p => p.Inventory == null || p.Inventory.Status == "Out of Stock");

            query = sort switch
            {
                "name-desc" => query.OrderByDescending(p => p.ProductName),
                "stock-asc" => query.OrderBy(p => p.Inventory!.QuantityOnHand),
                "stock-desc" => query.OrderByDescending(p => p.Inventory!.QuantityOnHand),
                "category" => query.OrderBy(p => p.Category).ThenBy(p => p.ProductName),
                _ => query.OrderBy(p => p.ProductName)
            };

            ViewData["Search"] = search;
            ViewData["Category"] = category;
            ViewData["StockStatus"] = stockStatus;
            ViewData["Sort"] = sort;
            ViewData["Archived"] = archived;
            ViewBag.Categories = await _context.Products.Select(p => p.Category).Where(c => c != "").Distinct().OrderBy(c => c).ToListAsync();
            return View(await query.ToListAsync());
        }

        [Authorize(Roles = "Admin,Warehouse")]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            ValidateImage(imageFile);
            if (!ModelState.IsValid) return View(product);

            product.ImageUrl = await SaveImageAsync(imageFile);
            product.MinimumOrderQuantity = Math.Max(1, product.MinimumOrderQuantity);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var inv = new Inventory { ProductID = product.ProductID, QuantityOnHand = 0, ReorderLevel = 10, Status = "Out of Stock" };
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
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            ValidateImage(imageFile);
            if (!ModelState.IsValid) return View(product);

            var existing = await _context.Products.FirstOrDefaultAsync(p => p.ProductID == product.ProductID);
            if (existing == null) return NotFound();

            existing.ProductName = product.ProductName;
            existing.SKU = product.SKU;
            existing.Category = product.Category;
            existing.Brand = product.Brand;
            existing.Description = product.Description;
            existing.MinimumOrderQuantity = Math.Max(1, product.MinimumOrderQuantity);
            existing.UnitPrice = product.UnitPrice;
            existing.UnitOfMeasure = product.UnitOfMeasure;
            if (imageFile != null && imageFile.Length > 0)
            {
                existing.ImageUrl = await SaveImageAsync(imageFile);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                var hasHistory = await _context.SalesOrderDetails.AnyAsync(item => item.ProductID == id)
                    || await _context.PurchaseOrderDetails.AnyAsync(item => item.ProductID == id)
                    || await _context.InventoryTransactions.AnyAsync(item => item.ProductID == id);
                if (hasHistory)
                {
                    TempData["Error"] = "This product has business history and cannot be permanently deleted. Archive it instead.";
                    return RedirectToAction(nameof(Index));
                }

                var inv = await _context.Inventories.FirstOrDefaultAsync(i => i.ProductID == id);
                if (inv != null) _context.Inventories.Remove(inv);

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Product deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int id, bool archived = true)
        {
            var product = await _context.Products.FirstOrDefaultAsync(item => item.ProductID == id);
            if (product == null) return NotFound();

            product.IsArchived = archived;
            await _context.SaveChangesAsync();
            TempData["Success"] = archived ? "Product archived successfully." : "Product restored successfully.";
            return RedirectToAction(nameof(Index), new { archived });
        }
    private async Task<string> SaveImageAsync(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0) return string.Empty;

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var directory = Path.Combine(_environment.WebRootPath, "uploads", "products");
        Directory.CreateDirectory(directory);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(directory, fileName);
        await using var stream = System.IO.File.Create(filePath);
        await imageFile.CopyToAsync(stream);
        return $"/uploads/products/{fileName}";
    }

    private void ValidateImage(IFormFile? imageFile)
    {
        if (imageFile == null || imageFile.Length == 0) return;

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension) || imageFile.Length > 5 * 1024 * 1024)
            ModelState.AddModelError("imageFile", "Product images must be JPG, PNG, or WEBP files smaller than 5 MB.");
    }
}
}