using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Application.Products;

public class ProductServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenProductsExist_ReturnsMappedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new(
                code: "PRD-001",
                name: "Mechanical Keyboard",
                category: "Technology",
                price: 80m,
                stock: 10
            ),
            new(
                code: "PRD-002",
                name: "Wireless Mouse",
                category: "Technology",
                price: 35m,
                stock: 20
            )
        };

        var repository = new FakeProductRepository(products);
        var service = new ProductService(repository);

        // Act
        var result = await service.GetAllAsync(search: null);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(products[0].Id, result[0].Id);
        Assert.Equal(products[0].Code, result[0].Code);
        Assert.Equal(products[0].Name, result[0].Name);
        Assert.Equal(products[0].Category, result[0].Category);
        Assert.Equal(products[0].Price, result[0].Price);
        Assert.Equal(products[0].Stock, result[0].Stock);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var repository = new FakeProductRepository();
        var service = new ProductService(repository);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        private readonly IReadOnlyList<Product> _products;

        public FakeProductRepository(
            IReadOnlyList<Product>? products = null)
        {
            _products = products ?? [];
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products);
        }

        public Task<Product?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var product = _products.FirstOrDefault(
                product => product.Id == id
            );

            return Task.FromResult(product);
        }
    }
}