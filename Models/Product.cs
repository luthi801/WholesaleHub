using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class Product
    {
        [Key]
        public int ProductID { get; set; }

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal Price { get; set; }
        public decimal UnitPrice { get; set; }
        public string Category { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        [StringLength(80)]
        public string Brand { get; set; } = string.Empty;
        [StringLength(120)]
        public string ImageUrl { get; set; } = string.Empty;
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;
        [Range(1, 100000)]
        public int MinimumOrderQuantity { get; set; } = 1;
        public bool IsArchived { get; set; }

        public Inventory? Inventory { get; set; }
    }
}