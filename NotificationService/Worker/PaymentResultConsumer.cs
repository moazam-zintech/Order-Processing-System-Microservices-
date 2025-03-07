// NotificationService/Workers/PaymentResultConsumer.cs
using Newtonsoft.Json;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Services;

namespace NotificationService.Workers
{
    public class PaymentResultConsumer : BackgroundService
    {
        private readonly ILogger<PaymentResultConsumer> _logger;
        private readonly RabbitMqService _rabbitMqService;

        public PaymentResultConsumer(ILogger<PaymentResultConsumer> logger, RabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqService.ConsumeMessage("payment_notification_queue", async message =>
            {
                try
                {
                    var paymentResult = JsonConvert.DeserializeObject<PaymentResult>(message);
                    if (paymentResult != null)
                    {
                        string notificationMessage = paymentResult.Success
                            ? $"Payment for Order {paymentResult.OrderId} was successful."
                            : $"Payment for Order {paymentResult.OrderId} failed: {paymentResult.Message}";

                        // Simulate sending email/SMS notification
                        _logger.LogInformation($"Sending Notification: {notificationMessage}");
                        // In a real application, you'd use an email/SMS library here
                    }
                    else
                    {
                        _logger.LogError("Could not deserialize payment result for notification.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing payment result for notification: {ex.Message}");
                }
            });

            await Task.CompletedTask;
        }
    }
}