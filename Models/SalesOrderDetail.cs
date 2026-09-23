using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class SalesOrderDetail
    {
        [Key]
        public int SODetailID { get; set; }

        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public int ProductID { get; set; }
        public Product? Product { get; set; }

        public int QuantityOrdered { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }
}