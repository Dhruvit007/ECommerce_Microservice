using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Domain.Entities
{
    public class ReturnItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid ReturnId { get; set; }
        public Return? Return { get; set; }

        public Guid OrderItemId { get; set; }
        public OrderItem? OrderItem { get; set; }

        public int ReturnedQuantity { get; set; }

        // Amount eligible for refund for this returned item

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundableAmount { get; set; }

    }
}
