using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class SalesOrderCreateViewModel
    {
        [Required(ErrorMessage = "Please select a customer")]
        public int CustomerID { get; set; }

        public List<OrderItemInput> Items { get; set; } = new();
    }
}