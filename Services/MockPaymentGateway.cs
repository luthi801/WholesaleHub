namespace WholesaleHub.Services
{
    public class PaymentRequest
    {
        public int OrderId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class PaymentGatewayResult
    {
        public bool IsSuccess { get; set; }
        public string Status { get; set; } = "Pending Payment";
        public string Message { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
    }

    public interface IPaymentGateway
    {
        Task<PaymentGatewayResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    }

    public class MockPaymentGateway : IPaymentGateway
    {
        public Task<PaymentGatewayResult> ProcessAsync(PaymentRequest request, CancellationToken cancellationToken = default)
        {
            var reference = $"MOCK-{request.OrderId}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            return Task.FromResult(new PaymentGatewayResult
            {
                IsSuccess = true,
                Status = "Payment Submitted",
                Message = "Mock payment gateway accepted the transaction and sent it to payment verification.",
                ReferenceNumber = reference
            });
        }
    }
}
