using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Text;
using WebApp.Entity.Data;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;
using WebApp.Services.Repository;
using Xunit;

namespace WebApp.Tests.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly ProductRepository _repository;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _repository = new ProductRepository(_context, _envMock.Object);
        }

        private Product CreateSampleProduct(bool active = true) =>
            new Product
            {
                Name = "Sample Product",
                Description = "Test description",
                Price = 100,
                Stock = 10,
                IsActive = active,
                CreatedAt = DateTime.UtcNow
            };

        private IFormFile CreateFakeImage(string fileName = "test.jpg")
        {
            var content = "Fake image content";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            return new FormFile(stream, 0, stream.Length, "file", fileName);
        }

        [Fact]
        public async Task CreateAsync_Should_Add_Product()
        {
            var product = CreateSampleProduct();

            var result = await _repository.CreateAsync(product, null);

            Assert.NotNull(result);
            Assert.Equal("Sample Product", result.Name);
            Assert.True(result.ProductId > 0);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Product_When_Exists()
        {
            var product = CreateSampleProduct();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = await _repository.GetByIdAsync(product.ProductId);

            Assert.NotNull(result);
            Assert.Equal(product.ProductId, result.ProductId);
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Null_When_NotFound()
        {
            var result = await _repository.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateAsync_Should_Update_Existing_Product()
        {
            var product = CreateSampleProduct();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            product.Name = "Updated Name";

            var result = await _repository.UpdateAsync(product, null);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
        }

        [Fact]
        public async Task UpdateAsync_Should_Return_Null_When_Product_NotFound()
        {
            var product = CreateSampleProduct();
            product.ProductId = 1234;

            var result = await _repository.UpdateAsync(product, null);

            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteAsync_Should_Set_IsActive_False()
        {
            var product = CreateSampleProduct();
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var result = await _repository.DeleteAsync(product.ProductId);

            var dbProduct = await _context.Products.FindAsync(product.ProductId);

            Assert.True(result);
            Assert.False(dbProduct.IsActive);
        }

        [Fact]
        public async Task DeleteAsync_Should_Return_False_When_NotFound()
        {
            var result = await _repository.DeleteAsync(12345);

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllAsync_Should_Return_Filtered_And_Sorted_Products()
        {
            _context.Products.AddRange(
                new Product { Name = "B Product", Description = "Desc B", Price = 200, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Product { Name = "A Product", Description = "Desc A", Price = 100, IsActive = true, CreatedAt = DateTime.UtcNow },
                new Product { Name = "Inactive", Description = "Desc Inactive", Price = 50, IsActive = false, CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var parameters = new ProductQueryParameters
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "name",
                SortOrder = "asc"
            };

            var result = await _repository.GetAllAsync(parameters);

            Assert.Equal(2, result.TotalCount); // Only active products
            Assert.Equal("A Product", result.Items.First().Name); // Sorted asc
        }


        [Fact]
        public async Task CreateAsync_Should_Save_Image_When_File_Provided()
        {
            var product = CreateSampleProduct();
            var file = CreateFakeImage();

            var result = await _repository.CreateAsync(product, file);

            Assert.NotNull(result.ImagePath);
            Assert.Contains("uploads", result.ImagePath);
        }
    }
}
