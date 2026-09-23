using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace WholesaleHub.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int AccountReceivableId { get; set; }

        [ForeignKey("AccountReceivableId")]
        public AccountsReceivable? AccountReceivable { get; set; }

        public decimal AmountPaid { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}