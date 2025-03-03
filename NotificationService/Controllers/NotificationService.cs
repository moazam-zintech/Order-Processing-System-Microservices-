// NotificationService/Controllers/NotificationController.cs
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Shared.Services;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/notifications")]  // API Gateway
    public class NotificationController : ControllerBase
    {
        private readonly ILogger<NotificationController> _logger;
        private readonly RabbitMqService _rabbitMqService;

        public NotificationController(ILogger<NotificationController> logger, RabbitMqService rabbitMqService)
        {
            _logger = logger;
            _rabbitMqService = rabbitMqService;
        }

        [HttpGet("test")]
        public IActionResult TestNotification()
        {
            _logger.LogInformation("Notification endpoint accessed.");
            return Ok("Notification service is running.");
        }
    }
}