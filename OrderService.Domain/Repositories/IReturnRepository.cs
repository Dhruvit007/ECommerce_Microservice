using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface IReturnRepository
    {
        Task<Return?> GetByIdAsync(Guid returnId);
        Task<List<Return>> GetByOrderIdAsync(Guid orderId);
        Task<Return?> AddAsync(Return orderReturn);
        Task<Return?> UpdateAsync(Return orderReturn);
        Task DeleteAsync(Guid returnId);
        Task<List<ReturnItem>> GetReturnItemsByReturnIdAsync(Guid returnId);
        Task<ReturnItem?> AddReturnItemAsync(ReturnItem returnItem);
        Task<ReturnItem?> UpdateReturnItemAsync(ReturnItem returnItem);
        Task DeleteReturnItemAsync(Guid returnItemId);
    }
}
