using OrderService.Domain.Entities;
namespace OrderService.Domain.Repositories
{
    public interface ICancellationRepository
    {
        Task<Cancellation?> GetByIdAsync(Guid cancellationId);
        Task<List<Cancellation>> GetByOrderIdAsync(Guid orderId);
        Task<Cancellation?> AddAsync(Cancellation cancellation);
        Task<Cancellation?> UpdateAsync(Cancellation cancellation);
        Task DeleteAsync(Guid cancellationId);
        Task<List<CancellationItem>> GetCancellationItemsByCancellationIdAsync(Guid cancellationId);
        Task<CancellationItem?> AddCancellationItemAsync(CancellationItem cancellationItem);
        Task<CancellationItem?> UpdateCancellationItemAsync(CancellationItem cancellationItem);
        Task DeleteCancellationItemAsync(Guid cancellationItemId);
    }
}
