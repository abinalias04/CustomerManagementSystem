using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;

namespace WebApp.Services.Repository
{
        public interface IProductRepository
        {

        Task<PagedResult<Product>> GetAllAsync(ProductQueryParameters parameters);
            Task<Product?> GetByIdAsync(int id);
            Task<Product> CreateAsync(Product product, IFormFile? imageFile);
            Task<Product?> UpdateAsync(Product product, IFormFile? imageFile);
            Task<bool> DeleteAsync(int id);   
        }
    
}
