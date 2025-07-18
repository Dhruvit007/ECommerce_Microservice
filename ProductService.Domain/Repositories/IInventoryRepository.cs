namespace ProductService.Domain.Repositories
{
    public interface IInventoryRepository
    {
        Task<bool> IsStockAvailableAsync(Guid productId, int quantity);
        Task UpdateStockAsync(Guid productId, int stockQuantity);
        Task IncreaseDecsreaseStockAsync(Guid productId, int quantityChange);
    }
}
