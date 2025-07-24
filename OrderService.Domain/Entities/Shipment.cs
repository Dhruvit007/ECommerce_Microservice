using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace OrderService.Domain.Entities
{
    [Index(nameof(OrderId))]
    public class Shipment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        [Required, MaxLength(150)]
        public string CarrierName { get; set; } = null!;

        [Required, MaxLength(200)]
        public string TrackingNumber { get; set; } = null!;

        [Required, MaxLength(100)]
        public string ShipmentStatus { get; set; } = null!;

        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        public ICollection<ShipmentItem> ShipmentItems { get; set; } = new List<ShipmentItem>();
    }
}
