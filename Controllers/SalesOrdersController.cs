using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Accountant,Warehouse,Customer")]
    public class SalesOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SalesOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.SalesOrders
                .Include(order => order.Customer)
                .AsQueryable();

            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null) return Forbid();
                query = query.Where(order => order.CustomerID == customer.CustomerID);
            }

            if (!string.IsNullOrWhiteSpace(status)) query = query.Where(order => order.OrderStatus == status);
            if (fromDate.HasValue) query = query.Where(order => order.OrderDate >= fromDate.Value.Date);
            if (toDate.HasValue) query = query.Where(order => order.OrderDate < toDate.Value.Date.AddDays(1));

            ViewBag.Status = status;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            var orders = await query.OrderByDescending(order => order.OrderDate).ToListAsync();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateOptions();
            return View(new CreateSalesOrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSalesOrderViewModel model)
        {
            if (User.IsInRole("Customer"))
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserID == userId);
                if (customer == null)
                {
                    ModelState.AddModelError(string.Empty, "Your customer profile is not configured yet.");
                }
                else
                {
                    model.CustomerID = customer.CustomerID;
                    ModelState.Remove(nameof(model.CustomerID));
                }
            }

            var items = model.Items.Where(item => item.ProductID > 0 && item.Quantity > 0 && item.UnitPrice > 0).ToList();
            if (items.Count == 0)
                ModelState.AddModelError(nameof(model.Items), "Add at least one valid product.");

            if (!ModelState.IsValid)
            {
                await LoadCreateOptions();
                return View(model);
            }

            var productIds = items.Select(item => item.ProductID).Distinct().ToList();
            var products = await _context.Products.Where(product => productIds.Contains(product.ProductID)).ToDictionaryAsync(product => product.ProductID);
            var inventories = await _context.Inventories.Where(i => productIds.Contains(i.ProductID)).ToDictionaryAsync(i => i.ProductID);
            foreach (var item in items)
            {
                if (!products.TryGetValue(item.ProductID, out var product) || !inventories.TryGetValue(item.ProductID, out var inventory) || inventory.QuantityOnHand < item.Quantity)
                {
                    ModelState.AddModelError(string.Empty, "One or more products do not have enough stock.");
                    await LoadCreateOptions();
                    return View(model);
                }
                item.UnitPrice = product.UnitPrice;
            }

            var customerRecord = await _context.Customers.FindAsync(model.CustomerID);
            if (customerRecord == null) return NotFound();

            var order = new SalesOrder
            {
                CustomerID = model.CustomerID,
                CustomerName = customerRecord.CompanyName,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending Payment",
                Status = "Pending Payment",
                PaymentStatus = "Pending Payment",
                TotalAmount = items.Sum(item => item.Quantity * item.UnitPrice)
            };

            foreach (var item in items)
            {
                order.SalesOrderDetails.Add(new SalesOrderDetail
                {
                    ProductID = item.ProductID,
                    QuantityOrdered = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Subtotal = item.Quantity * item.UnitPrice
                });
                inventories[item.ProductID].QuantityOnHand -= item.Quantity;
                inventories[item.ProductID].Status = inventories[item.ProductID].QuantityOnHand <= 0
                    ? "Out of Stock"
                    : inventories[item.ProductID].QuantityOnHand <= inventories[item.ProductID].ReorderLevel ? "Low Stock" : "In Stock";
                _context.InventoryTransactions.Add(new InventoryTransaction
                {
                    ProductID = item.ProductID,
                    UserID = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                    TransactionType = "Stock-Out",
                    Quantity = item.Quantity
                });
            }

            order.AccountsReceivable = new AccountsReceivable
            {
                CustomerID = model.CustomerID,
                TotalAmount = order.TotalAmount,
                PaymentStatus = "Pending"
            };

            _context.SalesOrders.Add(order);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Sales order saved to the database.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.SalesOrders
                .Include(item => item.Customer)
                .Include(item => item.SalesOrderDetails).ThenInclude(item => item.Product)
                .Include(item => item.Deliveries)
                .Include(item => item.AccountsReceivable)
                .FirstOrDefaultAsync(item => item.SalesOrderID == id);

            if (order == null) return NotFound();
            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null || order.CustomerID != customer.CustomerID) return Forbid();
            }
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Warehouse")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.SalesOrders.FindAsync(id);
            if (order == null) return NotFound();
            var next = order.OrderStatus switch
            {
                "Pending Payment" => new[] { "Payment Submitted", "Cancelled" },
                "Payment Submitted" => new[] { "Payment Verification", "Cancelled" },
                "Payment Verification" => new[] { "Paid", "Rejected", "Cancelled" },
                "Paid" => new[] { "Confirmed", "Cancelled" },
                "Confirmed" => new[] { "Processing", "Cancelled" },
                "Processing" => new[] { "Shipped", "Cancelled" },
                "Shipped" => new[] { "Completed" },
                _ => Array.Empty<string>()
            };
            if (!next.Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                TempData["Error"] = "That order status transition is not allowed.";
                return RedirectToAction(nameof(Details), new { id });
            }
            order.OrderStatus = status;
            order.Status = status;
            order.PaymentStatus = status switch
            {
                "Pending Payment" => "Pending Payment",
                "Payment Submitted" => "Payment Submitted",
                "Payment Verification" => "Payment Verification",
                "Paid" => "Paid",
                "Confirmed" => "Paid",
                _ => order.PaymentStatus
            };
            await _context.SaveChangesAsync();
            TempData["Success"] = "Order status updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task LoadCreateOptions()
        {
            ViewBag.Customers = await _context.Customers.OrderBy(customer => customer.CompanyName).ToListAsync();
            ViewBag.Products = await _context.Products.Where(product => !product.IsArchived).Include(product => product.Inventory).OrderBy(product => product.ProductName).ToListAsync();
        }

        private async Task<Customer?> GetCurrentCustomer()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdValue, out var userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(customer => customer.UserID == userId);
        }

        [HttpPost]
        public IActionResult CreateOrder([FromBody] WholesaleHub.Models.CreateSalesOrderInput input)
        {
            var items = input.Items ?? Enumerable.Empty<WholesaleHub.Models.SalesOrderItemInput>();

            // CS8604 fixed: non-null collection guarantees safe Sum evaluation
            decimal totalAmount = items.Sum(x => x.UnitPrice * x.Quantity);

            return Ok(new { Total = totalAmount });
        }
    }
}