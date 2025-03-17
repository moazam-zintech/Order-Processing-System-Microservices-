// PaymentService/Controllers/PaymentController.cs
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Services;

namespace PaymentService.Controllers
{
    [ApiController]
    [Route("api/payments")]  // API Gateway
    public class PaymentController : ControllerBase
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(RabbitMqService rabbitMqService, ILogger<PaymentController> logger)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
        }

        [HttpPost("process/{orderId}")]
        public IActionResult ProcessPayment(Guid orderId)
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

            return Ok(result); // Or a more appropriate status code
        }
    }
}