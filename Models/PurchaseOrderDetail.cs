using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class PurchaseOrderDetail
    {
        [Key]
        public int PODetailID { get; set; }

        public int PurchaseOrderID { get; set; }
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ProductID { get; set; }
        public Product? Product { get; set; }

        public int QuantityOrdered { get; set; }
        public decimal UnitCost { get; set; }
        public decimal Subtotal { get; set; }
    }
}