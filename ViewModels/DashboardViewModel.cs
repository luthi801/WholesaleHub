using WholesaleHub.Models;

namespace WholesaleHub.ViewModels
{
    public class DashboardViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public int CustomerTotalOrders { get; set; }
        public int CustomerPendingOrders { get; set; }
        public int CustomerCompletedOrders { get; set; }
        public List<SalesOrder> CustomerRecentOrders { get; set; } = new();

        public int TotalProducts { get; set; }
        public int LowStockItems { get; set; }
        public int OpenSalesOrders { get; set; }
        public decimal TotalARBalance { get; set; }
        public List<SalesOrder> RecentOrders { get; set; } = new();
        public List<Inventory> LowStockInventory { get; set; } = new();
        public Dictionary<string, int> InventoryStatusCounts { get; set; } = new();
        public Dictionary<string, decimal> SalesByMonth { get; set; } = new();
        public decimal PaidReceivables { get; set; }
        public decimal OutstandingReceivables { get; set; }
    }
}