using System.ComponentModel.DataAnnotations;

namespace WholesaleHub.ViewModels
{
    public class RecordPaymentViewModel
    {
        public int AR_ID { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal OutstandingBalance { get; set; }

        [Required]
        [Range(0.01, 1000000)]
        public decimal AmountToPay { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "Cash";
    }
}