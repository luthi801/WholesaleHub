using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using WholesaleHub.Data;
using WholesaleHub.Models;
using WholesaleHub.Services;
using WholesaleHub.ViewModels;

namespace WholesaleHub.Controllers
{
    [Authorize]
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGateway _paymentGateway;

        public PaymentsController(ApplicationDbContext context, IPaymentGateway paymentGateway)
        {
            _context = context;
            _paymentGateway = paymentGateway;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<CustomerPayment> query = _context.CustomerPayments
                .Include(payment => payment.SalesOrder)
                .Include(payment => payment.Customer)
                .Include(payment => payment.AccountsReceivable)
                .OrderByDescending(payment => payment.PaymentDate);

            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null) return Forbid();
                query = query.Where(payment => payment.CustomerID == customer.CustomerID);
            }

            var payments = await query.ToListAsync();
            return View(payments);
        }

        [Authorize(Roles = "Admin,Accountant,Customer")]
        public async Task<IActionResult> Checkout(int id)
        {
            var order = await _context.SalesOrders
                .Include(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(order => order.Customer)
                .Include(order => order.AccountsReceivable)
                .FirstOrDefaultAsync(order => order.SalesOrderID == id);

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
        [Authorize(Roles = "Admin,Accountant,Customer")]
        public async Task<IActionResult> Submit(PaymentSubmissionViewModel model)
        {
            if (model.OrderId <= 0)
            {
                ModelState.AddModelError(string.Empty, "A valid order is required to continue.");
                return BadRequest();
            }

            var order = await _context.SalesOrders
                .Include(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(order => order.AccountsReceivable)
                .FirstOrDefaultAsync(order => order.SalesOrderID == model.OrderId);

            if (order == null) return NotFound();

            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null || order.CustomerID != customer.CustomerID) return Forbid();
            }

            var amount = model.Amount > 0 ? model.Amount : order.TotalAmount;
            var gatewayResult = await _paymentGateway.ProcessAsync(new PaymentRequest
            {
                OrderId = order.SalesOrderID,
                CustomerId = order.CustomerID,
                Amount = amount,
                PaymentMethod = model.PaymentMethod,
                CustomerName = order.CustomerName,
                Notes = model.Notes
            });

            var receivable = order.AccountsReceivable ?? new AccountsReceivable
            {
                SalesOrderID = order.SalesOrderID,
                CustomerID = order.CustomerID,
                TotalAmount = order.TotalAmount,
                PaymentStatus = "Pending Payment"
            };

            if (order.AccountsReceivable == null)
            {
                _context.AccountsReceivables.Add(receivable);
                await _context.SaveChangesAsync();
            }

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var payment = new CustomerPayment
            {
                AR_ID = receivable.AR_ID,
                SalesOrderID = order.SalesOrderID,
                CustomerID = order.CustomerID,
                UserID = int.TryParse(userIdValue, out var userId) ? userId : 0,
                AmountPaid = amount,
                PaymentMethod = model.PaymentMethod,
                TransactionReference = gatewayResult.ReferenceNumber,
                PaymentStatus = gatewayResult.Status,
                PaymentDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                PaymentProofUrl = model.PaymentProofUrl,
                Notes = model.Notes,
                ProcessedBy = User.Identity?.Name ?? "Customer"
            };

            _context.CustomerPayments.Add(payment);

            order.PaymentStatus = gatewayResult.Status;
            order.OrderStatus = gatewayResult.Status == "Paid" ? "Confirmed" : "Payment Verification";
            order.Status = order.OrderStatus;
            receivable.TotalAmount = order.TotalAmount;
            receivable.AmountPaid = Math.Min(amount, order.TotalAmount);
            receivable.PaymentStatus = gatewayResult.Status == "Paid" ? "Paid" : (gatewayResult.Status == "Rejected" ? "Rejected" : "Payment Verification");

            await _context.SaveChangesAsync();

            TempData["Success"] = gatewayResult.Message;
            return RedirectToAction(nameof(Receipt), new { id = payment.PaymentID });
        }

        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Verification(string? status, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.CustomerPayments
                .Include(payment => payment.Customer)
                .Include(payment => payment.SalesOrder)
                .Include(payment => payment.AccountsReceivable)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(payment => payment.PaymentStatus == status);
            }

            if (fromDate.HasValue)
            {
                query = query.Where(payment => payment.PaymentDate >= fromDate.Value.Date);
            }

            if (toDate.HasValue)
            {
                query = query.Where(payment => payment.PaymentDate < toDate.Value.Date.AddDays(1));
            }

            var payments = await query.OrderByDescending(payment => payment.PaymentDate).ToListAsync();
            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Verify(int paymentId, string decision, string? notes)
        {
            var payment = await _context.CustomerPayments
                .Include(item => item.SalesOrder)
                .Include(item => item.AccountsReceivable)
                .FirstOrDefaultAsync(item => item.PaymentID == paymentId);

            if (payment == null) return NotFound();

            var isApproved = string.Equals(decision, "Approve", StringComparison.OrdinalIgnoreCase);
            payment.PaymentStatus = isApproved ? "Paid" : "Rejected";
            payment.ProcessedBy = User.Identity?.Name ?? "Admin";
            payment.Notes = string.IsNullOrWhiteSpace(notes) ? payment.Notes : notes;
            payment.PaymentDate = DateTime.UtcNow;

            if (payment.SalesOrder != null)
            {
                payment.SalesOrder.PaymentStatus = isApproved ? "Paid" : "Rejected";
                payment.SalesOrder.OrderStatus = isApproved ? "Confirmed" : "Pending Payment";
                payment.SalesOrder.Status = payment.SalesOrder.OrderStatus;
            }

            if (payment.AccountsReceivable != null)
            {
                payment.AccountsReceivable.AmountPaid = isApproved ? payment.AccountsReceivable.TotalAmount : 0m;
                payment.AccountsReceivable.PaymentStatus = isApproved ? "Paid" : "Rejected";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = isApproved
                ? "Payment approved. The order has been confirmed and the receipt is now available."
                : "Payment rejected and the order remains pending review.";

            return RedirectToAction(nameof(Verification));
        }

        public async Task<IActionResult> ReceiptForOrder(int orderId)
        {
            var payment = await _context.CustomerPayments
                .Include(item => item.SalesOrder)
                .ThenInclude(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(item => item.Customer)
                .Include(item => item.AccountsReceivable)
                .Where(item => item.SalesOrderID == orderId)
                .OrderByDescending(item => item.PaymentDate)
                .FirstOrDefaultAsync();

            if (payment == null)
            {
                return RedirectToAction(nameof(Checkout), new { id = orderId });
            }

            return RedirectToAction(nameof(Receipt), new { id = payment.PaymentID });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var payment = await _context.CustomerPayments
                .Include(item => item.SalesOrder)
                .ThenInclude(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(item => item.Customer)
                .Include(item => item.AccountsReceivable)
                .FirstOrDefaultAsync(item => item.PaymentID == id);

            if (payment == null) return NotFound();

            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null || payment.CustomerID != customer.CustomerID) return Forbid();
            }

            return View(payment);
        }

        public async Task<IActionResult> DownloadReceipt(int id)
        {
            var payment = await _context.CustomerPayments
                .Include(item => item.SalesOrder)
                .ThenInclude(order => order.SalesOrderDetails)
                .ThenInclude(detail => detail.Product)
                .Include(item => item.Customer)
                .Include(item => item.AccountsReceivable)
                .FirstOrDefaultAsync(item => item.PaymentID == id);

            if (payment == null) return NotFound();

            if (User.IsInRole("Customer"))
            {
                var customer = await GetCurrentCustomer();
                if (customer == null || payment.CustomerID != customer.CustomerID) return Forbid();
            }

            var order = payment.SalesOrder ?? new SalesOrder();
            var items = order.SalesOrderDetails ?? new List<SalesOrderDetail>();
            var itemRows = new StringBuilder();
            foreach (var item in items)
            {
                var productName = item.Product?.ProductName ?? "Product";
                itemRows.Append($"<tr><td>{productName}</td><td>{item.QuantityOrdered}</td><td>PHP {item.UnitPrice:N2}</td><td>PHP {item.Subtotal:N2}</td></tr>");
            }

            var statusLabel = payment.PaymentStatus == "Paid" ? "PAID" : "UNPAID";
            var html = $@"<!DOCTYPE html><html><head><title>WholesaleHub Receipt</title><meta charset='utf-8' /><style>body{{font-family:Arial,sans-serif;max-width:720px;margin:32px auto;color:#1f2937;}} .header{{text-align:center;border-bottom:2px solid #0d6efd;padding-bottom:12px;margin-bottom:18px;}} .badge{{display:inline-block;padding:6px 12px;background:#dcfce7;color:#166534;border-radius:999px;font-weight:bold;}} table{{width:100%;border-collapse:collapse;margin-top:16px;}} th,td{{padding:8px;border-bottom:1px solid #e5e7eb;text-align:left;}} .totals{{margin-top:18px;}} .totals td{{border:none;padding:4px 0;}} .footer{{margin-top:28px;font-size:12px;color:#6b7280;}} </style></head><body><div class='header'><h2>WholesaleHub</h2><div>Buhangin, Davao City, Philippines</div><div>+63 917 123 4567 | sales@wholesalehub.ph</div></div><h3>Receipt #{payment.PaymentID}</h3><div><strong>Order #:</strong> SO-{order.SalesOrderID}</div><div><strong>Transaction:</strong> {payment.TransactionReference}</div><div><strong>Customer:</strong> {payment.Customer?.CompanyName ?? order.CustomerName}</div><div><strong>Payment Date:</strong> {payment.PaymentDate:yyyy-MM-dd HH:mm}</div><div><strong>Payment Method:</strong> {payment.PaymentMethod}</div><div style='margin-top:12px;'><span class='badge'>{statusLabel}</span></div><table><thead><tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Subtotal</th></tr></thead><tbody>{itemRows}</tbody></table><table class='totals'><tr><td>Subtotal</td><td style='text-align:right;'>PHP {order.TotalAmount:N2}</td></tr><tr><td>Amount Paid</td><td style='text-align:right;'>PHP {payment.AmountPaid:N2}</td></tr><tr><td><strong>Grand Total</strong></td><td style='text-align:right;'><strong>PHP {order.TotalAmount:N2}</strong></td></tr></table><div class='footer'>Thank you for shopping with WholesaleHub. This receipt is connected to order SO-{order.SalesOrderID} and payment transaction {payment.TransactionReference}.</div></body></html>";

            return File(Encoding.UTF8.GetBytes(html), "text/html", $"receipt-{payment.PaymentID}.html");
        }

        private async Task<Customer?> GetCurrentCustomer()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId)) return null;
            return await _context.Customers.FirstOrDefaultAsync(customer => customer.UserID == userId);
        }
    }
}
