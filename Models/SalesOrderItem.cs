using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class SalesOrderItem
    {
        [Key]
        public int SalesOrderItemID { get; set; }

        [Required]
        public int SalesOrderID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalPrice { get; set; }

        [ForeignKey("SalesOrderID")]
        public virtual SalesOrder? SalesOrder { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}