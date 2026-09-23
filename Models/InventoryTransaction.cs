using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.Models
{
    public class InventoryTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        public int ProductID { get; set; }
        public Product? Product { get; set; }

        public int UserID { get; set; }
        public User? User { get; set; }

        [Required, StringLength(15)]
        public string TransactionType { get; set; } = string.Empty; // Stock-In, Stock-Out

        public int Quantity { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.Now;
    }
}