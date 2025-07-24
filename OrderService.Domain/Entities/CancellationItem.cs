using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class CancellationItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CancellationId { get; set; }
        public Cancellation? Cancellation { get; set; }

        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public int CancelledQuantity { get; set; }

        // Amount eligible for refund for this cancellation item
        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundableAmount { get; set; }
    }
}
