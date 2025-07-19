using Microsoft.AspNetCore.Mvc;
using ProductService.API.DTOs;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;

namespace ProductService.Api.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("availability/{productId:guid}/{quantity:int}")]
        public async Task<IActionResult> CheckAvailability(Guid productId, int quantity)
        {
            try
            {
                var isAvailable = await _inventoryService.IsStockAvailableAsync(productId, quantity);
                return Ok(ApiResponse<bool>.SuccessResponse(isAvailable));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAvailability");
                return StatusCode(500, ApiResponse<bool>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("update-stock")]
        public async Task<IActionResult> UpdateStock([FromBody] InventoryUpdateDTO inventoryUpdateDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<InventoryUpdateDTO>.FailResponse("Validation failed.", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()));

            try
            {
                await _inventoryService.UpdateStockAsync(inventoryUpdateDto);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Stock updated successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdateStock");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("decrease-stock")]
        public async Task<IActionResult> DecreaseStock([FromQuery] Guid productId, [FromQuery] int quantity)
        {
            if (quantity <= 0)
                return BadRequest(ApiResponse<string>.FailResponse("Quantity must be positive."));

            try
            {
                await _inventoryService.DecreaseStockAsync(productId, quantity);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Stock decreased successfully."));
            }
            catch (InvalidOperationException ioe)
            {
                return BadRequest(ApiResponse<string>.FailResponse(ioe.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DecreaseStock");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }

        [HttpPost("increase-stock")]
        public async Task<IActionResult> IncreaseStock([FromQuery] Guid productId, [FromQuery] int quantity)
        {
            if (quantity <= 0)
                return BadRequest(ApiResponse<string>.FailResponse("Quantity must be positive."));

            try
            {
                await _inventoryService.IncreaseStockAsync(productId, quantity);
                return Ok(ApiResponse<string>.SuccessResponse(string.Empty, "Stock increased successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IncreaseStock");
                return StatusCode(500, ApiResponse<string>.FailResponse("Internal server error."));
            }
        }
    }
}
