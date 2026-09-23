using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }
        public int PurchaseOrderId { get; set; }
        
        [ForeignKey("PurchaseOrderId")]
        public PurchaseOrder? PurchaseOrder { get; set; }

        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}