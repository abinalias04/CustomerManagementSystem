using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;
using WebApp.Services.Repository;

namespace WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repo;

        public ProductController(IProductRepository repo)
        {
            _repo = repo;
        }

        private string? BuildImageUrl(string? path) =>
            string.IsNullOrEmpty(path) ? null :
            $"{Request.Scheme}://{Request.Host}/{path}";

        private ProductResponseDto ToDto(Product p) => new ProductResponseDto
        {
            ProductId = p.ProductId,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Stock = p.Stock,
            ImageUrl = BuildImageUrl(p.ImagePath),
            IsActive = p.IsActive
        };
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortOrder = null)
        {
            var parameters = new ProductQueryParameters
            {
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _repo.GetAllAsync(parameters);

            return Ok(result);
        }
        [Authorize]
        [HttpGet("{id:int}")]

        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product == null) return NotFound();
            return Ok(ToDto(product));
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
 
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductResponseDto>> Create([FromForm] CreateProductDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock
            };

            var created = await _repo.CreateAsync(product, dto.ImageFile);
            return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, ToDto(created));
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]

        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ProductResponseDto>> Update(int id, [FromForm] UpdateProductDto dto)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            // Only update provided fields
            if (dto.Name != null) existing.Name = dto.Name;
            if (dto.Description != null) existing.Description = dto.Description;
            if (dto.Price.HasValue) existing.Price = dto.Price.Value;
            if (dto.Stock.HasValue) existing.Stock = dto.Stock.Value;
            if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;

            var updated = await _repo.UpdateAsync(existing, dto.ImageFile);
            return Ok(ToDto(updated));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
     
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repo.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
