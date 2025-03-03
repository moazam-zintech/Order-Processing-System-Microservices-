using System.ComponentModel.DataAnnotations;

namespace OrderService.Models
{
    public class OrderEntity
    {
        [Key]
        public Guid OrderId { get; set; }
        public string CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
