using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    [Index(nameof(CancellationStatusId))]
    public class Cancellation
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid CancellationStatusId { get; set; }
        public CancellationStatus? CancellationStatus { get; set; }

        public Guid ReasonId { get; set; }
        public ReasonMaster? Reason { get; set; }

        public bool IsPartial { get; set; }

        public DateTime RequestedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }

        [MaxLength(1000)]
        public string? Remarks { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }
        public ICollection<CancellationItem> CancellationItems { get; set; } = new List<CancellationItem>();
    }
}
