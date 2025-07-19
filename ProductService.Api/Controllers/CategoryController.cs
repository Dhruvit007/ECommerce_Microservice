using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<CategoryDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(ApiResponse<List<CategoryDTO>>.SuccessResponse(categories));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAll");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CategoryDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Category with id '{id}' not found"));

                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(category));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetById");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDTO createDto)
        {
            try
            {
                var result = await _categoryService.AddAsync(createDto);
                if (result == null)
                    return StatusCode(500, ApiResponse<string>.FailResponse("Failed to create category"));

                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(result, "Category created successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Create");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(CategoryUpdateDTO updateDto)
        {
            try
            {
                var result = await _categoryService.UpdateAsync(updateDto);
                if (result == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Category with id '{updateDto.Id}' not found"));

                return Ok(ApiResponse<CategoryDTO>.SuccessResponse(result, "Category updated successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Update");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var isDeleted = await _categoryService.DeleteAsync(id);
                if (isDeleted)
                    return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Category deleted successfully"));

                return NotFound(ApiResponse<string>.FailResponse("Category Not Found"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }
    }
}
