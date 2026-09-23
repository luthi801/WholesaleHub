using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                LowStockItems = await _context.Inventories.CountAsync(i => i.QuantityOnHand <= i.ReorderLevel),
                OpenSalesOrders = await _context.SalesOrders.CountAsync(s => s.OrderStatus != "Completed" && s.OrderStatus != "Cancelled"),
                TotalARBalance = await _context.AccountsReceivables.SumAsync(a => a.TotalAmount - a.AmountPaid),
                RecentOrders = await _context.SalesOrders.Include(s => s.Customer).OrderByDescending(s => s.OrderDate).Take(5).ToListAsync(),
                LowStockInventory = await _context.Inventories.Include(i => i.Product).Where(i => i.QuantityOnHand <= i.ReorderLevel).ToListAsync()
            };

            var inventoryStatuses = await _context.Inventories.Select(item => item.Status).ToListAsync();
            vm.InventoryStatusCounts = inventoryStatuses
                .GroupBy(status => string.IsNullOrWhiteSpace(status) ? "Unknown" : status)
                .ToDictionary(group => group.Key, group => group.Count());

            var salesForChart = await _context.SalesOrders.Select(order => new { order.OrderDate, order.TotalAmount }).ToListAsync();
            vm.SalesByMonth = salesForChart
                .GroupBy(order => new DateTime(order.OrderDate.Year, order.OrderDate.Month, 1))
                .OrderBy(group => group.Key)
                .ToDictionary(group => group.Key.ToString("MMM yyyy"), group => group.Sum(order => order.TotalAmount));

            var receivablesForChart = await _context.AccountsReceivables
                .Select(item => new { item.AmountPaid, item.TotalAmount })
                .ToListAsync();
            vm.PaidReceivables = receivablesForChart.Sum(item => item.AmountPaid);
            vm.OutstandingReceivables = receivablesForChart.Sum(item => item.TotalAmount - item.AmountPaid);

            if (User.IsInRole("Customer"))
            {
                if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId)) return Forbid();
                var customer = await _context.Customers.FirstOrDefaultAsync(item => item.UserID == userId);
                if (customer == null) return Forbid();

                var orders = _context.SalesOrders.Where(order => order.CustomerID == customer.CustomerID);
                vm.CustomerName = customer.ContactPerson;
                vm.CustomerTotalOrders = await orders.CountAsync();
                vm.CustomerPendingOrders = await orders.CountAsync(order => order.OrderStatus == "Pending");
                vm.CustomerCompletedOrders = await orders.CountAsync(order => order.OrderStatus == "Completed");
                vm.CustomerRecentOrders = await orders.OrderByDescending(order => order.OrderDate).Take(5).ToListAsync();
            }

            return View(vm);
        }
    }
}