using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public ProductService(IProductRepository repository, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _repository = repository;
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<ProductDTO?> GetByIdAsync(Guid id)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
                return null;

            var productDto = _mapper.Map<ProductDTO>(product);

            return productDto;
        }

        public async Task<List<ProductDTO>> GetAllAsync(int pageNumber = 1, int pageSize = 20)
        {
            var products = await _repository.GetAllAsync(pageNumber, pageSize);

            var productDTOs = _mapper.Map<List<ProductDTO>>(products);

            return productDTOs;
        }

        public async Task<List<ProductDTO>> SearchAsync(string? searchTerm, Guid? categoryId, decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 20)
        {
            var products = await _repository.SearchAsync(searchTerm, categoryId, minPrice, maxPrice, pageNumber, pageSize);
            var productDTOs = _mapper.Map<List<ProductDTO>>(products);
            return productDTOs;
        }

        public async Task<ProductDTO?> AddAsync(ProductCreateDTO createDto)
        {
            var product = _mapper.Map<Product>(createDto);
            product.Id = Guid.NewGuid();
            product.CreatedOn = DateTime.UtcNow;
            product.ModifiedOn = null;
            // Generate SKU dynamically before saving
            product.SKU = await GenerateProductSKUAsync(product);

            var createdProduct = await _repository.AddAsync(product);
            return createdProduct == null ? null : _mapper.Map<ProductDTO>(createdProduct);
        }

        public async Task<ProductDTO?> UpdateAsync(ProductUpdateDTO updateDto)
        {
            var existingProduct = await _repository.GetByIdAsync(updateDto.Id);
            if (existingProduct == null)
                return null;

            var product = _mapper.Map<Product>(updateDto);
            product.CreatedOn = existingProduct.CreatedOn; // keep original created time
            product.ModifiedOn = DateTime.UtcNow;

            // Generate SKU dynamically before saving
            product.SKU = await GenerateProductSKUAsync(product);

            var updatedProduct = await _repository.UpdateAsync(product);
            return updatedProduct == null ? null : _mapper.Map<ProductDTO>(updatedProduct);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _repository.DeleteAsync(id);
        }

        private async Task<string> GenerateProductSKUAsync(Product product)
        {
            // Get category by Id to fetch Category Name
            var category = await _categoryRepository.GetCategoryByIdAsync(product.CategoryId);
            if (category == null)
            {
                throw new InvalidOperationException("Category not found.");
            }

            string categoryPart = GetFirst3Letters(category.Name);
            string productPart = GetFirst3Letters(product.Name);
            string yearPart = DateTime.UtcNow.Year.ToString();

            // Get last 4 chars of Product GUID (without hyphens, uppercase)
            string guidSuffix = product.Id.ToString("N").Substring(28, 4).ToUpper();

            // Combine Parts to SKU
            return $"{categoryPart}-{productPart}-{yearPart}-{guidSuffix}";
        }

        private string GetFirst3Letters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "XXX"; // fallback

            return new string(input.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper().PadRight(3, 'X');
        }
    }
}
