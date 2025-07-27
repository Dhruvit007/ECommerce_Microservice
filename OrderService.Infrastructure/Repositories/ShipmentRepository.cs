using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.Repositories;
using OrderService.Infrastructure.Persistence;

namespace OrderService.Infrastructure.Repositories
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly OrderDbContext _dbContext;

        // Validate shipment status transitions
        private static readonly Dictionary<ShipmentStatusEnum, List<ShipmentStatusEnum>> AllowedTransitions = new Dictionary<ShipmentStatusEnum, List<ShipmentStatusEnum>>
        {
            //Pending => Shipped, Cancelled
            { ShipmentStatusEnum.Pending, new List<ShipmentStatusEnum> { ShipmentStatusEnum.Shipped, ShipmentStatusEnum.Cancelled } },
            
            //Shipped => InTransit, Cancelled
            { ShipmentStatusEnum.Shipped, new List<ShipmentStatusEnum> { ShipmentStatusEnum.InTransit, ShipmentStatusEnum.Cancelled } },
            
            //InTransit => OutForDelivery, Cancelled
            { ShipmentStatusEnum.InTransit, new List<ShipmentStatusEnum> { ShipmentStatusEnum.OutForDelivery, ShipmentStatusEnum.Cancelled } },
            
            //OutForDelivery => Delivered, Returned
            { ShipmentStatusEnum.OutForDelivery, new List<ShipmentStatusEnum> { ShipmentStatusEnum.Delivered, ShipmentStatusEnum.Returned } },
            
            //Delivered, Cancelled, Returned => Terminal
            { ShipmentStatusEnum.Delivered, new List<ShipmentStatusEnum>() },  // Terminal
            { ShipmentStatusEnum.Cancelled, new List<ShipmentStatusEnum>() },  // Terminal
            { ShipmentStatusEnum.Returned, new List<ShipmentStatusEnum>() }    // Terminal
        };

        public ShipmentRepository(OrderDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        // Retrieve all shipments for a given order
        public async Task<List<Shipment>> GetByOrderIdAsync(Guid orderId)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .Where(s => s.OrderId == orderId)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .OrderByDescending(s => s.EstimatedDeliveryDate)
                .ToListAsync();
        }

        // Check if Shipment exists by shipment Id
        public async Task<bool> ExistsAsync(Guid shipmentId)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .AnyAsync(s => s.Id == shipmentId);
        }

        // Add a new shipment record
        public async Task<Shipment> AddAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            shipment.IsDeleted = false;
            shipment.ShipmentStatusId = ShipmentStatusEnum.Pending;

            await _dbContext.Shipments.AddAsync(shipment);
            await _dbContext.SaveChangesAsync();

            return shipment;
        }

        // Update shipment details with status transition validation
        public async Task<Shipment?> UpdateAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            var existing = await _dbContext.Shipments
                .Include(s => s.ShipmentStatus)
                .FirstOrDefaultAsync(s => s.Id == shipment.Id);

            if (existing == null)
                return null;

            // Validate shipment status transitions
            var currentStatus = existing.ShipmentStatusId;
            var newStatus = shipment.ShipmentStatusId;

            if (currentStatus != newStatus)
            {
                if (!AllowedTransitions.TryGetValue(currentStatus, out var validNext) || !validNext.Contains(newStatus))
                    throw new InvalidOperationException($"Invalid status transition from {currentStatus} to {newStatus}.");

                existing.ShipmentStatusId = shipment.ShipmentStatusId;
            }

            // Update other shipment details
            existing.CarrierName = shipment.CarrierName;
            existing.TrackingNumber = shipment.TrackingNumber;
            existing.EstimatedDeliveryDate = shipment.EstimatedDeliveryDate;
            existing.DeliveredAt = shipment.DeliveredAt;

            await _dbContext.SaveChangesAsync();

            return existing;
        }

        // Get shipments by status with pagination
        public async Task<List<Shipment>> GetByStatusAsync(ShipmentStatusEnum shipmentStatusId, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .Where(s => s.ShipmentStatusId == shipmentStatusId)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .OrderByDescending(s => s.EstimatedDeliveryDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get shipments in a date range with pagination
        public async Task<List<Shipment>> GetByDateRangeAsync(DateTime from, DateTime to, int pageNumber = 1, int pageSize = 20)
        {
            return await _dbContext.Shipments
                .AsNoTracking()
                .Where(s => s.EstimatedDeliveryDate >= from && s.EstimatedDeliveryDate <= to && !s.IsDeleted)
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .OrderByDescending(s => s.EstimatedDeliveryDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        // Get shipment by tracking number
        public async Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
                return null;

            return await _dbContext.Shipments
                .AsNoTracking()
                .Include(s => s.ShipmentStatus)
                .Include(s => s.ShipmentItems)
                .FirstOrDefaultAsync(s => s.TrackingNumber == trackingNumber);
        }
    }
}
