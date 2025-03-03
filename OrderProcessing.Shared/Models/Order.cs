namespace OrderProcessing.Shared.Models
{
    public record Order
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";  // Pending, Processing, Paid, Failed, Shipped
    }
}
