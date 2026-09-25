using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int SalesOrderID { get; set; }
        public SalesOrder? SalesOrder { get; set; }

        public int CustomerID { get; set; }
        public Customer? Customer { get; set; }

        public int AccountReceivableId { get; set; }

        [ForeignKey("AccountReceivableId")]
        public AccountsReceivable? AccountReceivable { get; set; }

        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string TransactionReference { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Pending Payment";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PaymentProofUrl { get; set; }
        public string? Notes { get; set; }
        public string ProcessedBy { get; set; } = string.Empty;
    }
}