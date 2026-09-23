using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int QuantityOnHand { get; set; }

        public int ReorderLevel { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = "In Stock";

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}