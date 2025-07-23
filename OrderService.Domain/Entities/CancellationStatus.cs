using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class CancellationStatus
    {
        [Key]
        public Guid Id { get; set; }
        //Requested, Approved, Rejected, Refunded, Partially Refunded

        [Required, MaxLength(100)]
        public CancellationStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
