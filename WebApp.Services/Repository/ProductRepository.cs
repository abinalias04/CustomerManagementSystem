using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductRepository(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters parameters)
        {
            var query = _context.Products
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filter by product name
            if (!string.IsNullOrWhiteSpace(parameters.Search))
                query = query.Where(p => p.Name.Contains(parameters.Search));

            // Sorting
            if (!string.IsNullOrWhiteSpace(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "name" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "stock" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
                    "createdat" => parameters.SortOrder?.ToLower() == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.Name)
                };
            }
            else
            {
                query = query.OrderBy(p => p.Name);
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();

            return new PagedResult<Product>
            {
                Items = products,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }


        public async Task<Product?> GetByIdAsync(int id) =>
            await _context.Products.FindAsync(id);

        public async Task<Product> CreateAsync(Product product, IFormFile? imageFile)
        {
            if (imageFile != null)
                product.ImagePath = await SaveImageAsync(imageFile);

            product.CreatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product?> UpdateAsync(Product product, IFormFile? imageFile)
        {
            var existing = await _context.Products.FindAsync(product.ProductId);
            if (existing == null) return null;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Stock = product.Stock;
            existing.IsActive = product.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(existing.ImagePath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, existing.ImagePath.TrimStart('/'));
                    if (File.Exists(oldPath)) File.Delete(oldPath);
                }
                existing.ImagePath = await SaveImageAsync(imageFile);
            }

            _context.Products.Update(existing);
            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            product.IsActive = false;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string?> SaveImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return Path.Combine("uploads", fileName).Replace("\\", "/");
        }
    }
}
