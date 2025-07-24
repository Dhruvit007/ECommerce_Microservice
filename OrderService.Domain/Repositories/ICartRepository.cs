using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface ICartRepository
    {
        Task<Cart?> GetByUserIdAsync(Guid userId);
        Task<Cart?> AddAsync(Cart cart);
        Task<Cart?> UpdateAsync(Cart cart);
        Task RemoveProductFromCartAsync(Guid userId, Guid productId);
        Task DeleteAsyncByUserId(Guid userId);
    }
}
