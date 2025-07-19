using ProductService.Application.DTOs;
namespace ProductService.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> IsStockAvailableAsync(Guid productId, int quantity);
        Task UpdateStockAsync(InventoryUpdateDTO inventoryUpdateDto);
        Task DecreaseStockAsync(Guid productId, int quantity);
        Task IncreaseStockAsync(Guid productId, int quantity);
    }
}
