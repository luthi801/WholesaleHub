using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class AdjustStockViewModel
    {
        public int InventoryID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }

        [Required]
        [Range(0, 100000)]
        public int ReorderLevel { get; set; }

        [StringLength(100)]
        public string WarehouseLocation { get; set; } = string.Empty;

        [Required]
        public string AdjustmentType { get; set; } = "Stock-In"; // Stock-In, Stock-Out, Set

        [Required]
        [Range(0, 10000)]
        public int Quantity { get; set; }
    }
}