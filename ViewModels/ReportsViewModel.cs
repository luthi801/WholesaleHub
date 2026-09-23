using System.Collections.Generic;
using WholesaleHub.Models;

namespace WholesaleHub.ViewModels
{
    public class ReportsViewModel
    {
        public List<Inventory> InventoryList { get; set; } = new();
        public List<SalesOrder> SalesList { get; set; } = new();
        public List<PurchaseOrder> PurchaseList { get; set; } = new();
        public List<AccountsReceivable> ARList { get; set; } = new();
        public List<CustomerCrmInteraction> CrmList { get; set; } = new();
    }
}