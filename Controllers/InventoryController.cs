using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Warehouse")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? category, string? stockStatus, string? sort)
        {
            var query = _context.Inventories.Include(i => i.Product).Where(i => !i.Product!.IsArchived).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Product!.ProductName.Contains(search) || i.Product.SKU.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category)) query = query.Where(i => i.Product!.Category == category);
            if (stockStatus == "In Stock") query = query.Where(i => i.Status == "In Stock");
            if (stockStatus == "Low Stock") query = query.Where(i => i.Status == "Low Stock");
            if (stockStatus == "Out of Stock") query = query.Where(i => i.Status == "Out of Stock");

            query = sort switch
            {
                "name-desc" => query.OrderByDescending(i => i.Product!.ProductName),
                "stock-asc" => query.OrderBy(i => i.QuantityOnHand),
                "stock-desc" => query.OrderByDescending(i => i.QuantityOnHand),
                "status" => query.OrderBy(i => i.Status).ThenBy(i => i.Product!.ProductName),
                _ => query.OrderBy(i => i.Product!.ProductName)
            };

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.StockStatus = stockStatus;
            ViewBag.Sort = sort;
            ViewBag.Categories = await _context.Products.Select(p => p.Category).Where(c => c != "").Distinct().OrderBy(c => c).ToListAsync();
            return View(await query.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Adjust(int id)
        {
            var inv = await _context.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.InventoryID == id);
            if (inv == null) return NotFound();

            var vm = new AdjustStockViewModel
            {
                InventoryID = inv.InventoryID,
                ProductName = inv.Product?.ProductName ?? "",
                QuantityOnHand = inv.QuantityOnHand,
                ReorderLevel = inv.ReorderLevel,
                WarehouseLocation = inv.WarehouseLocation,
                Quantity = 0
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(AdjustStockViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var inv = await _context.Inventories.Include(i => i.Product).FirstOrDefaultAsync(i => i.InventoryID == vm.InventoryID);
            if (inv == null) return NotFound();

            inv.ReorderLevel = vm.ReorderLevel;
            inv.WarehouseLocation = vm.WarehouseLocation?.Trim() ?? string.Empty;

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (vm.AdjustmentType == "Stock-In")
            {
                inv.QuantityOnHand += vm.Quantity;
                _context.InventoryTransactions.Add(new InventoryTransaction { ProductID = inv.ProductID, UserID = userId, TransactionType = "Stock-In", Quantity = vm.Quantity });
            }
            else if (vm.AdjustmentType == "Stock-Out")
            {
                inv.QuantityOnHand = Math.Max(0, inv.QuantityOnHand - vm.Quantity);
                _context.InventoryTransactions.Add(new InventoryTransaction { ProductID = inv.ProductID, UserID = userId, TransactionType = "Stock-Out", Quantity = vm.Quantity });
            }

            inv.Status = GetStatus(inv.QuantityOnHand, inv.ReorderLevel);
            inv.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stock updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private static string GetStatus(int quantityOnHand, int reorderLevel)
        {
            return quantityOnHand <= 0 ? "Out of Stock" : quantityOnHand <= reorderLevel ? "Low Stock" : "In Stock";
        }
    }
}