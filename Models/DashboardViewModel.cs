using System.Collections.Generic;

namespace WholesaleHub.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal TotalSalesValue { get; set; }
        public int PendingDeliveries { get; set; }
        public decimal OutstandingBalance { get; set; }

        public IEnumerable<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();
        public IEnumerable<LowStockDto> LowStockProducts { get; set; } = new List<LowStockDto>();
    }

    public class RecentOrderDto
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class LowStockDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
    }
}