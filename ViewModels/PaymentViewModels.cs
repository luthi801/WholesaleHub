namespace WholesaleHub.ViewModels
{
    public class PaymentSubmissionViewModel
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = "GCash";
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public string ExpiryDate { get; set; } = string.Empty;
        public string SecurityCode { get; set; } = string.Empty;
        public string? PaymentProofUrl { get; set; }
        public string? Notes { get; set; }
    }

    public class PaymentVerificationViewModel
    {
        public int PaymentId { get; set; }
        public string Decision { get; set; } = "Approve";
        public string? Notes { get; set; }
    }
}
