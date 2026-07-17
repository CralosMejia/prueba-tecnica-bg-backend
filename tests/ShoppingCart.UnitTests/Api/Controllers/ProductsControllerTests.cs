using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Controllers;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Api.Controllers;

public class ProductsControllerTests
{
    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ReturnsProblemDetails404()
    {
        // Arrange
        var service = new FakeProductService();
        var controller = new ProductsController(service);

        // Act
        var result = await controller.GetById(
            Guid.NewGuid(),
            CancellationToken.None
        );

        // Assert
        var notFoundResult =
            Assert.IsType<NotFoundObjectResult>(result.Result);

        var problemDetails =
            Assert.IsType<ProblemDetails>(notFoundResult.Value);

        Assert.Equal(404, problemDetails.Status);
        Assert.Equal(
            "Product not found",
            problemDetails.Title
        );
    }

    private sealed class FakeProductService : IProductService
    {
        public Task<IReadOnlyList<ProductResponse>> GetAllAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ProductResponse> products = [];

            return Task.FromResult(products);
        }

        public Task<IReadOnlyList<ProductResponse>>
        GetAllForAdministrationAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "This test does not use administrative product listing."
            );
        }

        public Task<ProductResponse?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<ProductResponse?>(null);
        }

        public Task<ProductResponse> CreateAsync(
            CreateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "This fake is only used for public product controller tests."
            );
        }

        public Task<ProductResponse> UpdateAsync(
            Guid id,
            UpdateProductRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "This fake is only used for public product controller tests."
            );
        }

        public Task ToggleActivityStatusAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException(
                "This fake is only used for public product controller tests."
            );
        }
    }
}