
using OrderService.Domain.Enums;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    public class ReturnStatus
    {
        [Key]
        public Guid Id { get; set; }

        //Requested, Item Received, Inspected, Approved, Rejected, Refunded, Completed
        [Required, MaxLength(100)]
        public ReturnStatusEnum StatusName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
