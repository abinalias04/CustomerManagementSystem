using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CartRepository(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private string _hostUrl =>
            $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";

        // Add item to cart 
        public async Task<CartResponseDto> AddToCartAsync(int userId, AddToCartDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                throw new Exception("Product not found");

            if (!product.IsActive || product.Stock <= 0)
                throw new Exception("This product is not available");

            if (dto.Quantity <= 0)
                throw new Exception("Quantity must be greater than 0");

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, LastUpdated = DateTime.UtcNow, IsDeleted = false };
                _context.Carts.Add(cart);
            }

            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            int totalRequested = dto.Quantity + (existingItem?.Quantity ?? 0);

            if (totalRequested > product.Stock)
                throw new Exception($"Not enough stock available. Only {product.Stock} left.");

            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            cart.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Always reload with Includes after SaveChanges
            cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

            return new CartResponseDto
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price,
                    SubTotal = i.Quantity * i.Product.Price,
                    ProductDescription = i.Product.Description,
                    ProductImageUrl = i.Product.ImagePath
                }).ToList(),
                Total = cart.Items.Sum(i => i.Quantity * i.Product.Price)
            };
        }

        // Update item in cart
        public async Task<CartResponseDto> UpdateCartItemAsync(int userId, UpdateCartItemDto dto)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == dto.CartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            if (dto.Quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                //  Do not compare with stock here
                //  Only update quantity
                cartItem.Quantity = dto.Quantity;
                cartItem.Cart.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return await GetCartAsync(userId);
        }

        // Remove item from cart
        public async Task<CartResponseDto> RemoveCartItemAsync(int userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.Cart.UserId == userId);

            if (cartItem == null)
                throw new Exception("Cart item not found");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return await GetCartAsync(userId);
        }

        // Get current cart
        public async Task<CartResponseDto> GetCartAsync(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);

            if (cart == null)
                return new CartResponseDto { CartId = 0, Total = 0 };

            return new CartResponseDto
            {
                CartId = cart.CartId,
                Items = cart.Items.Select(i => new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    ProductId = i.ProductId,
                    ProductName = i.Product.Name,
                    Stock = i.Product.Stock,
                    ProductDescription = i.Product.Description,
                    ProductImageUrl = i.Product.ImagePath != null ? $"{_hostUrl}/{i.Product.ImagePath}" : null,
                    UnitPrice = i.Product.Price,
                    
                    Quantity = i.Quantity,
                    SubTotal = i.Product.Price * i.Quantity
                }).ToList(),
                Total = cart.Items.Sum(i => i.Product.Price * i.Quantity)
            };
        }
    }
}
