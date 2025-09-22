using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Entity.Dto;
using WebApp.Services.Repository;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _cartRepo;

        public CartController(ICartRepository cartRepo)
        {
            _cartRepo = cartRepo;
        }

        private int GetUserId()
        {
            if (HttpContext.Items["UserId"] == null)
                throw new Exception("User ID not found in context. Ensure JWT is valid.");

            return int.Parse(HttpContext.Items["UserId"]!.ToString()!);
        }
        [Authorize(Roles = "Customer")]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            var result = await _cartRepo.AddToCartAsync(GetUserId(), dto);
            return Ok(result);
        }
        [Authorize(Roles = "Customer")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCart(UpdateCartItemDto dto)
        {
            var result = await _cartRepo.UpdateCartItemAsync(GetUserId(), dto);
            return Ok(result);
        }
        [Authorize(Roles = "Customer")]
        [HttpDelete("remove/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var result = await _cartRepo.RemoveCartItemAsync(GetUserId(), cartItemId);
            return Ok(result);
        }
        [Authorize(Roles = "Customer")]
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var result = await _cartRepo.GetCartAsync(GetUserId());
            return Ok(result);
        }
    }
}
