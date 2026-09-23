using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Accountant")]
    public class AccountsReceivableController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountsReceivableController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var list = await _context.AccountsReceivables
                .Include(a => a.Customer)
                .Include(a => a.SalesOrder)
                .Include(a => a.Payments)
                .ToListAsync();
            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> RecordPayment(int id)
        {
            var ar = await _context.AccountsReceivables.Include(a => a.Customer).FirstOrDefaultAsync(a => a.AR_ID == id);
            if (ar == null) return NotFound();

            var vm = new RecordPaymentViewModel
            {
                AR_ID = ar.AR_ID,
                CustomerName = ar.Customer?.CompanyName ?? "",
                TotalAmount = ar.TotalAmount,
                OutstandingBalance = ar.OutstandingBalance,
                AmountToPay = ar.OutstandingBalance
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(RecordPaymentViewModel vm)
        {
            var ar = await _context.AccountsReceivables.FirstOrDefaultAsync(a => a.AR_ID == vm.AR_ID);
            if (ar == null) return NotFound();

            if (vm.AmountToPay <= 0 || vm.AmountToPay > ar.OutstandingBalance)
            {
                ModelState.AddModelError("AmountToPay", "Invalid payment amount.");
                return View(vm);
            }

            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var payment = new CustomerPayment
            {
                AR_ID = ar.AR_ID,
                UserID = userId,
                AmountPaid = vm.AmountToPay,
                PaymentMethod = vm.PaymentMethod,
                PaymentDate = DateTime.Now
            };

            _context.CustomerPayments.Add(payment);
            ar.AmountPaid += vm.AmountToPay;

            if (ar.OutstandingBalance <= 0)
                ar.PaymentStatus = "Paid";
            else
                ar.PaymentStatus = "Partial";

            await _context.SaveChangesAsync();
            TempData["Success"] = "Payment recorded successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}