using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WholesaleHub.Data;

namespace WholesaleHub.Controllers
{
    [Authorize(Roles = "Admin,Warehouse")]
    public class DeliveriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeliveriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var deliveries = await _context.Deliveries
                .Include(d => d.SalesOrder).ThenInclude(s => s!.Customer)
                .OrderByDescending(d => d.DeliveryDate)
                .ToListAsync();
            return View(deliveries);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int deliveryId, string deliveryStatus, string trackingNumber)
        {
            var delivery = await _context.Deliveries.Include(d => d.SalesOrder).FirstOrDefaultAsync(d => d.DeliveryID == deliveryId);
            if (delivery == null) return NotFound();

            delivery.DeliveryStatus = deliveryStatus;
            delivery.TrackingNumber = trackingNumber;

            if (deliveryStatus == "Delivered" && delivery.SalesOrder != null)
            {
                delivery.SalesOrder.OrderStatus = "Completed";
            }
            else if (deliveryStatus == "In Transit" && delivery.SalesOrder != null)
            {
                delivery.SalesOrder.OrderStatus = "Processing";
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Delivery status updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}