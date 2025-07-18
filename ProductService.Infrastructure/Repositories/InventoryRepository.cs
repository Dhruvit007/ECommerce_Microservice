using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ProductDbContext _dbContext;
        public InventoryRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> IsStockAvailableAsync(Guid productId, int quantity)
        {
            var product = await _dbContext.Products
                .FirstOrDefaultAsync(prd => prd.Id == productId && prd.IsActive == true);

            if (product == null)
                return false;

            return product.StockQuantity >= quantity;
        }

        public async Task UpdateStockAsync(Guid productId, int stockQuantity)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity = stockQuantity;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task IncreaseDecsreaseStockAsync(Guid productId, int quantityChange)
        {
            var product = await _dbContext.Products.FindAsync(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
