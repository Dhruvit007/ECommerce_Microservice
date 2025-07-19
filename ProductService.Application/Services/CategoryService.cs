using AutoMapper;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Domain.Repositories;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid id)
        {
            var category = await _repository.GetCategoryByIdAsync(id);

            if (category == null)
            {
                return null;
            }

            var categoryDTO = _mapper.Map<CategoryDTO>(category);
            return categoryDTO;
        }

        public async Task<List<CategoryDTO>> GetAllAsync()
        {
            var categories = await _repository.GetAllCategoriesAsync();
            return _mapper.Map<List<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> AddAsync(CategoryCreateDTO createDto)
        {
            Category category = _mapper.Map<Category>(createDto);

            category.Id = Guid.NewGuid();
            category.CreatedOn = DateTime.UtcNow;
            category.ModifiedOn = null;

            var createdCategory = await _repository.AddCategoryAsync(category);
            return createdCategory == null ? null : _mapper.Map<CategoryDTO>(createdCategory);
        }

        public async Task<CategoryDTO?> UpdateAsync(CategoryUpdateDTO updateDto)
        {
            var existingCategory = await _repository.GetCategoryByIdAsync(updateDto.Id);
            if (existingCategory == null)
                return null;

            var category = _mapper.Map<Category>(updateDto);
            category.CreatedOn = existingCategory.CreatedOn;
            category.ModifiedOn = DateTime.UtcNow;

            var updatedCategory = await _repository.UpdateCategoryAsync(category);
            return updatedCategory == null ? null : _mapper.Map<CategoryDTO>(updatedCategory);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await _repository.DeleteCategoryAsync(id);
        }
    }
}
