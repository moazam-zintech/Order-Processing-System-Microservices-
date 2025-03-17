using OrderProcessing.Shared.Enums;

namespace OrderProcessing.Shared.Models
{
    public record Order
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;  // Pending, Processing, Paid, Failed, Shipped
    }
}
