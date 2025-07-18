using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using ProductService.Domain.Repositories;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ProductDbContext _dbContext;
        public CategoryRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Category?> GetCategoryByIdAsync(Guid categoryId)
        {
            return await _dbContext.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == categoryId);
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _dbContext.Categories
                .Include(c => c.SubCategories)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Category?> AddCategoryAsync(Category category)
        {
            await _dbContext.Categories.AddAsync(category); //Added
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<Category?> UpdateCategoryAsync(Category category)
        {
            _dbContext.Categories.Update(category); //Modified
            await _dbContext.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _dbContext.Categories.FindAsync(categoryId);
            if (category != null)
            {
                _dbContext.Categories.Remove(category); //Deleted
                await _dbContext.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
