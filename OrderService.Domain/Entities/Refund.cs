using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    [Index(nameof(CancellationId))]
    [Index(nameof(ReturnId))]
    public class Refund
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid? CancellationId { get; set; }
        public Cancellation? Cancellation { get; set; }

        public Guid? ReturnId { get; set; }
        public Return? Return { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundBaseAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundedTaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundedDiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundedShippingCharges { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RefundAmount { get; set; }

        [Required, MaxLength(100)]
        public string PaymentMethod { get; set; } = null!;

        public DateTime RefundDate { get; set; }

        public Guid RefundStatusId { get; set; }
        public RefundStatus? RefundStatus { get; set; }

        [MaxLength(200)]
        public string? TransactionReference { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<RefundItem> RefundItems { get; set; } = new List<RefundItem>();
    }
}
