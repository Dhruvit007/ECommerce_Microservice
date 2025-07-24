using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid ProductId { get; set; }

        [Required, MaxLength(250)]
        public string ProductName { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountApplied { get; set; }
        public int Quantity { get; set; }
    }
}
