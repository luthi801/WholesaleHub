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

        public async Task<IActionResult> Index(string search)
        {
            var query = _context.Inventories.Include(i => i.Product).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.Product!.ProductName.Contains(search) || i.Product.SKU.Contains(search));
            }

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

            inv.LastUpdated = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Stock updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}