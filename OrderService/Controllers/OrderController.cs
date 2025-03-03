// OrderService/Controllers/OrderController.cs
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Shared.Models;
using OrderProcessing.Shared.Services;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/orders")]  // Use "api/orders" for API Gateway consistency
    public class OrderController : ControllerBase
    {
        private readonly RabbitMqService _rabbitMqService;
        private readonly ILogger<OrderController> _logger;
        private readonly OrderDbContext _context;

        public OrderController(RabbitMqService rabbitMqService, ILogger<OrderController> logger, OrderDbContext context)
        {
            _rabbitMqService = rabbitMqService;
            _logger = logger;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            order.OrderId = Guid.NewGuid();
            order.Status = "Created"; // Set initial status

            var orderEntity = new OrderEntity
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            };

            _context.Orders.Add(orderEntity);
            await _context.SaveChangesAsync();

            _rabbitMqService.PublishMessage("order_exchange", order);

            _logger.LogInformation($"Order Created: {order.OrderId}");
            return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var orderEntity = await _context.Orders.FindAsync(id);

            if (orderEntity == null)
            {
                return NotFound();
            }

            var order = new Order
            {
                OrderId = orderEntity.OrderId,
                CustomerId = orderEntity.CustomerId,
                TotalAmount = orderEntity.TotalAmount,
                Status = orderEntity.Status
            };
            return Ok(order);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid id, string status)
        {
            var orderEntity = await _context.Orders.FindAsync(id);
            if (orderEntity == null)
            {
                return NotFound();
            }

            orderEntity.Status = status;
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Order {id} status updated to {status}");
            return Ok(orderEntity);
        }
    }
}