namespace OrderProcessing.Shared.Models
{
    public record RabbitMqConfig
    {
        public string HostName { get; set; } = "localhost"; // Default
        public string OrderExchangeName { get; set; } = "order_exchange";
        public string PaymentExchangeName { get; set; } = "payment_exchange";
        public string NotificationExchangeName { get; set; } = "notification_exchange";
        public string OrderPaymentQueueName { get; set; } = "order_payment_queue";
        public string PaymentNotificationQueueName { get; set; } = "payment_notification_queue";
        public string OrderUpdateQueueName { get; set; } = "order_update_queue";
    }
}
