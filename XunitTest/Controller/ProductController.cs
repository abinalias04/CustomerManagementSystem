using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApp.Controllers;
using WebApp.Entity.Dto;
using WebApp.Entity.Models;
using WebApp.Services.Repository;

namespace WebApp.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductRepository> _repoMock;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _repoMock = new Mock<IProductRepository>();
            _controller = new ProductController(_repoMock.Object);

            // Fake HttpContext to support Request.Scheme & Request.Host in BuildImageUrl
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost", 5000);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task GetAll_Should_Return_Ok_With_Result()
        {
            // Arrange
            var pagedResult = new PagedResult<Product>
            {
                Items = new List<Product>
                {
                    new Product { ProductId = 1, Name = "Test", Description = "Desc", Price = 100, Stock = 5, IsActive = true }
                },
                TotalCount = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _repoMock.Setup(r => r.GetAllAsync(It.IsAny<ProductQueryParameters>()))
                .ReturnsAsync(pagedResult);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returned = Assert.IsType<PagedResult<Product>>(okResult.Value);
            Assert.Single(returned.Items);
        }

        [Fact]
        public async Task GetById_Should_Return_Ok_When_Product_Found()
        {
            var product = new Product { ProductId = 1, Name = "Test", Description = "Desc", Price = 50, Stock = 10, IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("Test", dto.Name);
        }

        [Fact]
        public async Task GetById_Should_Return_NotFound_When_Product_NotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Create_Should_Return_CreatedAtAction()
        {
            var dto = new CreateProductDto
            {
                Name = "New Product",
                Description = "New Desc",
                Price = 100,
                Stock = 10
            };

            var createdProduct = new Product
            {
                ProductId = 1,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                IsActive = true
            };

            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Product>(), It.IsAny<IFormFile>()))
                .ReturnsAsync(createdProduct);

            var result = await _controller.Create(dto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ProductResponseDto>(createdAt.Value);
            Assert.Equal("New Product", response.Name);
        }

        [Fact]
        public async Task Update_Should_Return_Ok_When_Successful()
        {
            var existing = new Product { ProductId = 1, Name = "Old", Description = "Old Desc", Price = 50, Stock = 10, IsActive = true };

            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(existing, null)).ReturnsAsync(existing);

            var dto = new UpdateProductDto { Name = "Updated" };

            var result = await _controller.Update(1, dto);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var response = Assert.IsType<ProductResponseDto>(okResult.Value);
            Assert.Equal("Updated", response.Name);
        }

        [Fact]
        public async Task Update_Should_Return_NotFound_When_Product_NotExist()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product)null);

            var dto = new UpdateProductDto { Name = "Updated" };
            var result = await _controller.Update(99, dto);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Delete_Should_Return_NoContent_When_Successful()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Delete_Should_Return_NotFound_When_Failed()
        {
            _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(false);

            var result = await _controller.Delete(1);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
