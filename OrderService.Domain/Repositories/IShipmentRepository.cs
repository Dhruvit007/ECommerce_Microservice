using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface IShipmentRepository
    {
        Task<List<Shipment>?> GetByOrderIdAsync(Guid orderId);
        Task<Shipment?> GetByOrderItemIdAsync(Guid orderId, Guid OrderItemId);
        Task<Shipment?> AddAsync(Shipment shipment);
        Task<Shipment?> UpdateAsync(Shipment shipment);
        Task DeleteAsync(Guid logisticsId);
    }
}
