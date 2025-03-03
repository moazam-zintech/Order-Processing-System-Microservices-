namespace OrderProcessing.Shared.Models
{
    public record PaymentResult
    {
        public Guid OrderId { get; set; }
        public bool Success { get; set; }
        public string? Message { get; set; }  // Optional error message
    }
}
