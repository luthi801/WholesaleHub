using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class PurchaseOrderCreateViewModel
    {
        [Required(ErrorMessage = "Please select a supplier")]
        public int SupplierID { get; set; }

        public List<OrderItemInput> Items { get; set; } = new();
    }

    public class OrderItemInput
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceOrCost { get; set; }
    }
}