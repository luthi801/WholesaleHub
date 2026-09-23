using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class StockTransaction
    {
        [Key]
        public int StockTransID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public int? CreatedBy { get; set; }

        [Required, StringLength(15)]
        public string TransType { get; set; } = string.Empty; // Stock-In, Stock-Out

        public int Quantity { get; set; }

        [StringLength(30)]
        public string ReferenceType { get; set; } = string.Empty; // PO, SO, Manual

        [StringLength(30)]
        public string ReferenceID { get; set; } = string.Empty;

        public DateTime TransDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string Notes { get; set; } = string.Empty;

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [ForeignKey("CreatedBy")]
        public virtual User? User { get; set; }
    }
}