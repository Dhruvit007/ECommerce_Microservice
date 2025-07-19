using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.API.Controllers
{
    [ApiController]
    [Route("api/product-images")]
    public class ProductImageController : ControllerBase
    {
        private readonly IProductImageService _imageService;
        private readonly ILogger<ProductImageController> _logger;

        public ProductImageController(IProductImageService imageService, ILogger<ProductImageController> logger)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpGet("by-product/{productId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByProductId(Guid productId)
        {
            try
            {
                var images = await _imageService.GetByProductIdAsync(productId);
                return Ok(ApiResponse<List<ProductImageDTO>>.SuccessResponse(images));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetByProductId");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var image = await _imageService.GetByIdAsync(id);
                if (image == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Image with id '{id}' not found"));

                return Ok(ApiResponse<ProductImageDTO>.SuccessResponse(image));
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
        public async Task<IActionResult> Add([FromBody] ProductImageCreateDTO createDto)
        {
            try
            {
                var result = await _imageService.AddAsync(createDto);
                if (result == null)
                    return StatusCode(500, ApiResponse<string>.FailResponse("Failed to add image"));

                return Ok(ApiResponse<ProductImageDTO>.SuccessResponse(result, "Image added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Add");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromBody] ProductImageUpdateDTO updateDto)
        {
            try
            {
                var result = await _imageService.UpdateAsync(updateDto);
                if (result == null)
                    return NotFound(ApiResponse<string>.FailResponse($"Image with id '{updateDto.Id}' not found"));

                return Ok(ApiResponse<ProductImageDTO>.SuccessResponse(result, "Image updated successfully"));
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
                await _imageService.DeleteAsync(id);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Image deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Delete");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error"));
            }
        }
    }
}
