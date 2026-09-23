using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderID { get; set; }

        public int Id { get; set; }
        public int SupplierID { get; set; }
        public int UserID { get; set; }
        public Supplier? Supplier { get; set; }
        public User? User { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }

        public ICollection<PurchaseOrderDetail> PurchaseOrderDetails { get; set; } = new List<PurchaseOrderDetail>();
        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
    }
}