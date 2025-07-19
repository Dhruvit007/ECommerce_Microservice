using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;

namespace ProductService.Application.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> IsStockAvailableAsync(Guid productId, int quantity)
        {
            return await _repository.IsStockAvailableAsync(productId, quantity);
        }

        public async Task UpdateStockAsync(InventoryUpdateDTO inventoryUpdateDto)
        {
            await _repository.UpdateStockAsync(inventoryUpdateDto.ProductId, inventoryUpdateDto.QuantityChange);
        }

        public async Task DecreaseStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            // Check if enough stock is available
            bool available = await IsStockAvailableAsync(productId, quantity);
            if (!available)
                throw new InvalidOperationException("Insufficient stock to decrease.");

            await _repository.IncreaseDecsreaseStockAsync(productId, -quantity);
        }

        public async Task IncreaseStockAsync(Guid productId, int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive", nameof(quantity));

            await _repository.IncreaseDecsreaseStockAsync(productId, quantity);
        }
    }
}
