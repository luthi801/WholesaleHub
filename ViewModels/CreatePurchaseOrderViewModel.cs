using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class CreatePurchaseOrderViewModel
    {
        [Required]
        public int SupplierID { get; set; }

        public List<PurchaseOrderItemInput> Items { get; set; } = new();
    }

    public class PurchaseOrderItemInput
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice => UnitCost;
    }

    public class CreatePurchaseOrderInput
    {
        public int SupplierID { get; set; }
        public IEnumerable<PurchaseOrderItemInput>? Items { get; set; }
    }
}