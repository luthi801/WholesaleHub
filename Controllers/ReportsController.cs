using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Data;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Accountant,Warehouse")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vm = new ReportsViewModel
            {
                InventoryList = await _context.Inventories.Include(i => i.Product).ToListAsync(),
                SalesList = await _context.SalesOrders.Include(s => s.Customer).ToListAsync(),
                PurchaseList = await _context.PurchaseOrders.Include(p => p.Supplier).ToListAsync(),
                ARList = await _context.AccountsReceivables.Include(a => a.Customer).ToListAsync(),
                CrmList = await _context.CustomerCrmInteractions.Include(c => c.Customer).ToListAsync()
            };

            return View(vm);
        }
    }
}