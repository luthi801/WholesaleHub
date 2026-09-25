using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class CustomerPayment
    {
        [Key]
        public int PaymentID { get; set; }

        public int AR_ID { get; set; }

        [ForeignKey(nameof(AR_ID))]
        public AccountsReceivable? AccountsReceivable { get; set; }

        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }

        public int UserID { get; set; }
        public User? User { get; set; }

        public decimal AmountPaid { get; set; }

        [Required, StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash";

        [StringLength(50)]
        public string TransactionReference { get; set; } = string.Empty;

        [StringLength(30)]
        public string PaymentStatus { get; set; } = "Pending Payment";

        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? PaymentProofUrl { get; set; }
        public string? Notes { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
    }
}