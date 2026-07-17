using ShoppingCart.Application.Carts;
using ShoppingCart.Application.Common.Exceptions;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Application.Carts;

public sealed class CartServiceTests
{
    [Fact]
    public async Task GetAsync_WhenCartDoesNotExist_ReturnsEmptyCart()
    {
        // Arrange
        var cartRepository = new FakeCartRepository();

        var service = new CartService(
            cartRepository,
            new FakeProductRepository()
        );

        // Act
        var response = await service.GetAsync(
            Guid.NewGuid(),
            CancellationToken.None
        );

        // Assert
        Assert.Empty(response.Items);
        Assert.Equal(0m, response.Subtotal);
        Assert.Equal(0m, response.Discount);
        Assert.Equal(0m, response.Total);
    }

    [Fact]
    public async Task AddItemAsync_WithValidProduct_CreatesCartAndReturnsTotals()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var product = CreateProduct(
            price: 25m,
            stock: 10
        );

        var cartRepository = new FakeCartRepository();

        var service = new CartService(
            cartRepository,
            new FakeProductRepository(product)
        );

        var request = new AddCartItemRequest(
            product.Id,
            Quantity: 3
        );

        // Act
        var response = await service.AddItemAsync(
            userId,
            request,
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(cartRepository.Cart);
        Assert.Equal(userId, cartRepository.Cart.UserId);
        Assert.Equal(1, cartRepository.AddCalls);
        Assert.Equal(1, cartRepository.SaveChangesCalls);

        var item = Assert.Single(response.Items);

        Assert.Equal(product.Id, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(25m, item.UnitPrice);
        Assert.Equal(75m, item.Subtotal);

        Assert.Equal(75m, response.Subtotal);
        Assert.Equal(0m, response.Discount);
        Assert.Equal(75m, response.Total);
    }

    [Fact]
    public async Task AddItemAsync_WhenProductDoesNotExist_ThrowsResourceNotFoundException()
    {
        // Arrange
        var cartRepository = new FakeCartRepository();

        var service = new CartService(
            cartRepository,
            new FakeProductRepository()
        );

        var request = new AddCartItemRequest(
            Guid.NewGuid(),
            Quantity: 1
        );

        // Act
        var action = () => service.AddItemAsync(
            Guid.NewGuid(),
            request,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<ResourceNotFoundException>(
            action
        );

        Assert.Equal(0, cartRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task AddItemAsync_WhenAccumulatedQuantityExceedsStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var product = CreateProduct(
            price: 20m,
            stock: 5
        );

        var cart = new Cart(userId);
        cart.AddItem(product.Id, quantity: 4);

        var cartRepository = new FakeCartRepository(cart);

        var service = new CartService(
            cartRepository,
            new FakeProductRepository(product)
        );

        var request = new AddCartItemRequest(
            product.Id,
            Quantity: 2
        );

        // Act
        var action = () => service.AddItemAsync(
            userId,
            request,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<InsufficientStockException>(
            action
        );

        Assert.Equal(0, cartRepository.SaveChangesCalls);
        Assert.Equal(
            4,
            Assert.Single(cart.Items).Quantity
        );
    }

    [Fact]
    public async Task UpdateQuantityAsync_WithValidQuantity_UpdatesCart()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var product = CreateProduct(
            price: 30m,
            stock: 10
        );

        var cart = new Cart(userId);
        cart.AddItem(product.Id, quantity: 2);

        var cartRepository = new FakeCartRepository(cart);

        var service = new CartService(
            cartRepository,
            new FakeProductRepository(product)
        );

        var request = new UpdateCartItemRequest(
            Quantity: 4
        );

        // Act
        var response = await service.UpdateQuantityAsync(
            userId,
            product.Id,
            request,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(4, Assert.Single(cart.Items).Quantity);
        Assert.Equal(1, cartRepository.SaveChangesCalls);

        Assert.Equal(120m, response.Subtotal);
        Assert.Equal(12m, response.Discount);
        Assert.Equal(108m, response.Total);
    }

    [Fact]
    public async Task UpdateQuantityAsync_WhenQuantityExceedsStock_ThrowsInsufficientStockException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var product = CreateProduct(
            price: 30m,
            stock: 5
        );

        var cart = new Cart(userId);
        cart.AddItem(product.Id, quantity: 2);

        var cartRepository = new FakeCartRepository(cart);

        var service = new CartService(
            cartRepository,
            new FakeProductRepository(product)
        );

        var request = new UpdateCartItemRequest(
            Quantity: 6
        );

        // Act
        var action = () => service.UpdateQuantityAsync(
            userId,
            product.Id,
            request,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<InsufficientStockException>(
            action
        );

        Assert.Equal(2, Assert.Single(cart.Items).Quantity);
        Assert.Equal(0, cartRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task ClearAsync_WithExistingCart_RemovesAllItems()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var firstProduct = CreateProduct(
            code: "P-001"
        );

        var secondProduct = CreateProduct(
            code: "P-002"
        );

        var cart = new Cart(userId);
        cart.AddItem(firstProduct.Id, quantity: 1);
        cart.AddItem(secondProduct.Id, quantity: 2);

        var cartRepository = new FakeCartRepository(cart);

        var service = new CartService(
            cartRepository,
            new FakeProductRepository(
                firstProduct,
                secondProduct
            )
        );

        // Act
        var response = await service.ClearAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        Assert.Empty(cart.Items);
        Assert.Empty(response.Items);
        Assert.Equal(0m, response.Total);
        Assert.Equal(1, cartRepository.SaveChangesCalls);
    }

    private static Product CreateProduct(
        string code = "P-001",
        decimal price = 10m,
        int stock = 10)
    {
        return new Product(
            code,
            name: $"Product {code}",
            category: "Test",
            price,
            stock
        );
    }

    private sealed class FakeCartRepository(
        Cart? initialCart = null)
        : ICartRepository
    {
        public Cart? Cart { get; private set; } =
            initialCart;

        public int AddCalls { get; private set; }

        public int SaveChangesCalls { get; private set; }

        public Task<Cart?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var result = Cart?.UserId == userId
                ? Cart
                : null;

            return Task.FromResult(result);
        }

        public Task AddAsync(
            Cart cart,
            CancellationToken cancellationToken = default)
        {
            Cart = cart;
            AddCalls++;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCalls++;

            return Task.CompletedTask;
        }
    }

private sealed class FakeProductRepository(
    params Product[] products)
    : IProductRepository
{
    private readonly List<Product> _products =
        products.ToList();

    public Task<IReadOnlyList<Product>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> result =
            _products
                .Where(product => product.IsActive)
                .ToList();

        return Task.FromResult(result);
    }

    public Task<Product?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(
            currentProduct =>
                currentProduct.Id == id &&
                currentProduct.IsActive
        );

        return Task.FromResult(product);
    }

    public Task<Product?> GetByIdForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var product = _products.FirstOrDefault(
            currentProduct =>
                currentProduct.Id == id
        );

        return Task.FromResult(product);
    }

    public Task<IReadOnlyList<Product>>
        GetByIdsForUpdateAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Product> matchingProducts =
            _products
                .Where(product =>
                    ids.Contains(product.Id)
                )
                .ToList();

        return Task.FromResult(
            matchingProducts
        );
    }

    public Task<bool> ExistsByCodeAsync(
        string code,
        Guid? excludeProductId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = _products.Any(product =>
            string.Equals(
                product.Code,
                code,
                StringComparison.OrdinalIgnoreCase
            ) &&
            product.Id != excludeProductId
        );

        return Task.FromResult(exists);
    }

    public Task<Product> AddAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        /*
         * CartService nunca crea productos.
         * Este método existe únicamente para cumplir
         * el contrato de IProductRepository.
         */
        throw new NotSupportedException(
            "Cart tests do not support adding products."
        );
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        /*
         * CartService guarda mediante ICartRepository,
         * no mediante IProductRepository.
         */
        return Task.CompletedTask;
    }
}
}