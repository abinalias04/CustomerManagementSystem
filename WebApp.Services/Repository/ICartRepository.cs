using WebApp.Entity.Dto;

namespace WebApp.Services.Repository
{
    public interface ICartRepository
    {
        Task<CartResponseDto> AddToCartAsync(int userId, AddToCartDto dto);
        Task<CartResponseDto> UpdateCartItemAsync(int userId, UpdateCartItemDto dto);
        Task<CartResponseDto> RemoveCartItemAsync(int userId, int cartItemId);
        Task<CartResponseDto> GetCartAsync(int userId);
    }
}
