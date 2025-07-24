using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid orderId);
        Task<List<Order>> GetByUserIdAsync(Guid userId, int pageNumber = 1, int pageSize = 10);
        Task<Order?> AddAsync(Order order);
        Task<Order?> UpdateAsync(Order order);
        Task DeleteAsync(Guid orderId);
    }
}
