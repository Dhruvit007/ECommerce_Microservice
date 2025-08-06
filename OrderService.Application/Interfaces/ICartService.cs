using OrderService.Application.DTOs.Cart;
namespace OrderService.Application.Interfaces
{
    public interface ICartService
    {
        Task<List<CartItemResponseDTO>> AddItemToCartAsync(AddCartItemRequestDTO request);
        Task<List<CartItemResponseDTO>> UpdateCartItemAsync(UpdateCartItemRequestDTO request);
        Task<List<CartItemResponseDTO>> RemoveCartItemAsync(RemoveCartItemRequestDTO request);
        Task ClearCartAsync(ClearCartRequestDTO request);
        Task<List<CartItemResponseDTO>> GetCartItemsAsync(Guid? userId);
        Task<List<CartItemResponseDTO>> MergeCartsAsync(Guid targetUserId, Guid sourceUserId);
    }
}
