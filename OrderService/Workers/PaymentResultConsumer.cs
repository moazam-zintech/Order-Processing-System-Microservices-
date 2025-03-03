// OrderService/Workers/PaymentResultConsumer.cs
using Newtonsoft.Json;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Services;
using OrderService.Data;

namespace OrderService.Workers
{
    public class PaymentResultConsumer : BackgroundService
    {
        private readonly ILogger<PaymentResultConsumer> _logger;
        private readonly RabbitMqService _rabbitMqService;
        private readonly IServiceProvider _serviceProvider;  // Inject IServiceProvider

        public PaymentResultConsumer(ILogger<PaymentResultConsumer> logger, RabbitMqService rabbitMqService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqService.ConsumeMessage("order_update_queue", async message =>
            {
                try
                {
                    var paymentResult = JsonConvert.DeserializeObject<PaymentResult>(message);
                    if (paymentResult != null)
                    {
                        using (var scope = _serviceProvider.CreateScope()) // Create a scope
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();  // Resolve DbContext

                            var order = await dbContext.Orders.FindAsync(paymentResult.OrderId);
                            if (order != null)
                            {
                                order.Status = paymentResult.Success ? "Paid" : "Payment Failed";
                                await dbContext.SaveChangesAsync();
                                _logger.LogInformation($"Order {paymentResult.OrderId} status updated to {order.Status}.");
                            }
                            else
                            {
                                _logger.LogWarning($"Order {paymentResult.OrderId} not found in database.");
                            }
                        }
                    }
                    else
                    {
                        _logger.LogError("Could not deserialize payment result.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing payment result: {ex.Message}");
                }
            });

            await Task.CompletedTask;
        }
    }
}