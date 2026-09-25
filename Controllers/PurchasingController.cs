using System.Linq;
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
    public class PurchasingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PurchasingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.PurchaseOrders
                .Include(order => order.Supplier)
                .Include(order => order.PurchaseOrderDetails)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateOptions();
            return View(new CreatePurchaseOrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePurchaseOrderViewModel model)
        {
            var items = model.Items.Where(item => item.ProductID > 0 && item.Quantity > 0 && item.UnitCost > 0).ToList();
            if (items.Count == 0)
                ModelState.AddModelError(nameof(model.Items), "Add at least one valid product.");

            if (!ModelState.IsValid)
            {
                await LoadCreateOptions();
                return View(model);
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var order = new PurchaseOrder
            {
                SupplierID = model.SupplierID,
                UserID = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = items.Sum(item => item.Quantity * item.UnitCost)
            };

            foreach (var item in items)
            {
                order.PurchaseOrderDetails.Add(new PurchaseOrderDetail
                {
                    ProductID = item.ProductID,
                    QuantityOrdered = item.Quantity,
                    UnitCost = item.UnitCost,
                    Subtotal = item.Quantity * item.UnitCost
                });
            }

            _context.PurchaseOrders.Add(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Purchase order saved to the database.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.PurchaseOrders
                .Include(item => item.Supplier)
                .Include(item => item.PurchaseOrderDetails).ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(item => item.PurchaseOrderID == id);

            return order == null ? NotFound() : View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.PurchaseOrders
                .Include(item => item.PurchaseOrderDetails)
                .FirstOrDefaultAsync(item => item.PurchaseOrderID == id);
            if (order == null) return NotFound();

            if (status == "Received" && order.Status != "Received")
            {
                var productIds = order.PurchaseOrderDetails.Select(item => item.ProductID).ToList();
                var inventories = await _context.Inventories.Where(item => productIds.Contains(item.ProductID)).ToDictionaryAsync(item => item.ProductID);
                foreach (var item in order.PurchaseOrderDetails)
                {
                    if (inventories.TryGetValue(item.ProductID, out var inventory))
                    {
                        inventory.QuantityOnHand += item.QuantityOrdered;
                        inventory.Status = inventory.QuantityOnHand <= 0 ? "Out of Stock" : inventory.QuantityOnHand <= inventory.ReorderLevel ? "Low Stock" : "In Stock";
                        _context.InventoryTransactions.Add(new InventoryTransaction
                        {
                            ProductID = item.ProductID,
                            UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                            TransactionType = "Stock-In",
                            Quantity = item.QuantityOrdered
                        });
                    }
                }
            }

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Purchase order status updated and inventory synchronized.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task LoadCreateOptions()
        {
            ViewBag.Suppliers = await _context.Suppliers.OrderBy(supplier => supplier.SupplierName).ToListAsync();
            ViewBag.Products = await _context.Products.Where(product => !product.IsArchived).OrderBy(product => product.ProductName).ToListAsync();
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreatePurchaseOrderInput input)
        {
            var items = input.Items ?? Enumerable.Empty<PurchaseOrderItemInput>();

            // CS8604 fixed: non-null collection guarantees safe Sum evaluation
            decimal totalAmount = items.Sum(x => x.UnitPrice * x.Quantity);

            return Ok(new { Total = totalAmount });
        }
    }
}