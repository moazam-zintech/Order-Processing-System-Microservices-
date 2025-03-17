// PaymentService/Workers/OrderConsumer.cs
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Services;

namespace PaymentService.Workers
{
    public class OrderConsumer : BackgroundService
    {
        private readonly ILogger<OrderConsumer> _logger;
        private readonly RabbitMqService _rabbitMqService;

        public OrderConsumer(ILogger<OrderConsumer> logger, RabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqService.ConsumeMessage("order_payment_queue", async message =>
            {
                try
                {
                    var order = JsonConvert.DeserializeObject<Order>(message);
                    if (order != null)
                    {
                        _logger.LogInformation($"Received Order: {order.OrderId} for processing payment.");
                        await ProcessPayment(order.OrderId);  // Call the internal payment processing method
                    }
                    else
                    {
                        _logger.LogError("Could not deserialize order.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing order: {ex.Message}");
                }
            });

            await Task.CompletedTask;
        }

        private async Task<IActionResult> ProcessPayment(Guid orderId)
        {
            // Simulate payment processing (success or failure)
            bool success = Random.Shared.Next(0, 2) == 0; // 50% chance of success

            PaymentResult result = new PaymentResult
            {
                OrderId = orderId,
                Success = success,
                Message = success ? null : "Simulated payment failure."
            };

            _rabbitMqService.PublishMessage("payment_exchange", result);

            _logger.LogInformation($"Payment processed for Order {orderId}: Success = {success}");

            return new OkObjectResult(result); // Or a more appropriate status code
        }
    }
}