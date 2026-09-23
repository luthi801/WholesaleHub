using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class CreateSalesOrderViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        public List<SalesOrderItemInput> Items { get; set; } = new();
    }

    public class SalesOrderItemInput
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}