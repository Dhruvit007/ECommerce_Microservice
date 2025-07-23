using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class RefundStatus
    {
        [Key]
        public Guid Id { get; set; }

        // Examples: Pending, Completed, Failed, PartiallyRefunded
        [Required, MaxLength(100)]
        public RefundStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
