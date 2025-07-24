using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class OrderStatusHistory
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid OrderStatusId { get; set; }
        public OrderStatus? OrderStatus { get; set; }

        public DateTime ChangedAt { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }
    }
}
