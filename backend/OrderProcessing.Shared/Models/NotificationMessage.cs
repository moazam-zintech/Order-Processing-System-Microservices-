namespace OrderProcessing.Shared.Models
{
    public record NotificationMessage
    {
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public string Message { get; set; }
    }
}
