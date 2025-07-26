﻿using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Domain.Repositories
{
    public interface IShipmentRepository
    {
        // Retrieve all shipments for a given order
        Task<List<Shipment>> GetByOrderIdAsync(Guid orderId);

        //Check if Shipment exists
        Task<bool> ExistsAsync(Guid shipmentId);

        // Add a new shipment record
        Task<Shipment> AddAsync(Shipment shipment);

        // Update shipment details (e.g., status, estimated delivery)
        Task<Shipment?> UpdateAsync(Shipment shipment);

        // Get all shipments with a particular status (for reporting/dashboard)
        Task<List<Shipment>> GetByStatusAsync(ShipmentStatusEnum shipmentStatusId, int pageNumber = 1, int pageSize = 20);

        // Get all shipments in a date range (for analytics)
        Task<List<Shipment>> GetByDateRangeAsync(DateTime from, DateTime to, int pageNumber = 1, int pageSize = 20);

        // Get shipment by tracking number (common query for external tracking)
        Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber);
    }
}

