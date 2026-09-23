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

        public int UserID { get; set; }
        public User? User { get; set; }

        public decimal AmountPaid { get; set; }

        [Required, StringLength(20)]
        public string PaymentMethod { get; set; } = "Cash"; // Cash, GCash, Bank Transfer

        public DateTime PaymentDate { get; set; } = DateTime.Now;
    }
}