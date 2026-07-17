using ShoppingCart.Application.Carts;
using ShoppingCart.Application.Common.Exceptions;
using ShoppingCart.Application.Common.Persistence;
using ShoppingCart.Application.Orders;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Orders;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.UnitTests.Application.Orders;

public sealed class OrderServiceTests
{
    [Fact]
    public async Task CheckoutAsync_WithValidCart_CreatesOrderDecreasesStockClearsCartAndCommits()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var keyboard = CreateProduct(
            code: "PROD-001",
            price: 50m,
            stock: 5
        );

        var mouse = CreateProduct(
            code: "PROD-002",
            price: 20m,
            stock: 3
        );

        var cart = new Cart(userId);
        cart.AddItem(keyboard.Id, quantity: 2);
        cart.AddItem(mouse.Id, quantity: 1);

        var cartRepository =
            new FakeCartRepository(cart);

        var productRepository =
            new FakeProductRepository(
                keyboard,
                mouse
            );

        var orderRepository =
            new FakeOrderRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new OrderService(
            cartRepository,
            productRepository,
            orderRepository,
            unitOfWork
        );

        // Act
        var response = await service.CheckoutAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(3, keyboard.Stock);
        Assert.Equal(2, mouse.Stock);

        Assert.Empty(cart.Items);

        Assert.Equal(1, orderRepository.AddCalls);

        Assert.Equal(1, unitOfWork.ExecuteCalls);
        Assert.Equal(1, unitOfWork.CommitCalls);
        Assert.Equal(0, unitOfWork.RollbackCalls);

        // El repositorio del carrito no debe guardar directamente.
        Assert.Equal(0, cartRepository.SaveChangesCalls);

        var order = Assert.Single(
            orderRepository.Orders
        );

        Assert.Equal(userId, order.UserId);
        Assert.Equal(2, order.Items.Count);

        Assert.Equal(120m, response.Subtotal);
        Assert.Equal(12m, response.Discount);
        Assert.Equal(108m, response.Total);

        Assert.Equal(2, response.Items.Count);
    }

    [Fact]
    public async Task CheckoutAsync_WithEmptyCart_ThrowsBusinessConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var cart = new Cart(userId);

        var cartRepository =
            new FakeCartRepository(cart);

        var orderRepository =
            new FakeOrderRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new OrderService(
            cartRepository,
            new FakeProductRepository(),
            orderRepository,
            unitOfWork
        );

        // Act
        var action = () => service.CheckoutAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            BusinessConflictException
        >(action);

        Assert.Empty(orderRepository.Orders);

        Assert.Equal(1, unitOfWork.ExecuteCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
        Assert.Equal(1, unitOfWork.RollbackCalls);

        Assert.Equal(0, cartRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task CheckoutAsync_WhenAnyProductHasInsufficientStock_DoesNotModifyStateOrCommit()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var availableProduct = CreateProduct(
            code: "PROD-001",
            price: 50m,
            stock: 10
        );

        var unavailableProduct = CreateProduct(
            code: "PROD-002",
            price: 20m,
            stock: 1
        );

        var cart = new Cart(userId);
        cart.AddItem(
            availableProduct.Id,
            quantity: 2
        );

        cart.AddItem(
            unavailableProduct.Id,
            quantity: 2
        );

        var cartRepository =
            new FakeCartRepository(cart);

        var orderRepository =
            new FakeOrderRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new OrderService(
            cartRepository,
            new FakeProductRepository(
                availableProduct,
                unavailableProduct
            ),
            orderRepository,
            unitOfWork
        );

        // Act
        var action = () => service.CheckoutAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            InsufficientStockException
        >(action);

        // Ningún producto debe modificarse parcialmente.
        Assert.Equal(10, availableProduct.Stock);
        Assert.Equal(1, unavailableProduct.Stock);

        // El carrito debe conservarse.
        Assert.Equal(2, cart.Items.Count);

        Assert.Empty(orderRepository.Orders);

        Assert.Equal(1, unitOfWork.ExecuteCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
        Assert.Equal(1, unitOfWork.RollbackCalls);

        Assert.Equal(0, cartRepository.SaveChangesCalls);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderDoesNotBelongToUser_ThrowsResourceNotFoundException()
    {
        // Arrange
        var service = new OrderService(
            new FakeCartRepository(),
            new FakeProductRepository(),
            new FakeOrderRepository(),
            new FakeUnitOfWork()
        );

        // Act
        var action = () => service.GetByIdAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            ResourceNotFoundException
        >(action);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyOrdersBelongingToUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var orderRepository =
            new FakeOrderRepository();

        var userOrder = Order.Create(
            userId,
            new[]
            {
                new OrderLine(
                    Guid.NewGuid(),
                    "PROD-001",
                    "Keyboard",
                    50m,
                    1
                )
            },
            DateTime.UtcNow
        );

        var otherUserOrder = Order.Create(
            otherUserId,
            new[]
            {
                new OrderLine(
                    Guid.NewGuid(),
                    "PROD-002",
                    "Mouse",
                    20m,
                    1
                )
            },
            DateTime.UtcNow
        );

        await orderRepository.AddAsync(userOrder);
        await orderRepository.AddAsync(otherUserOrder);

        var service = new OrderService(
            new FakeCartRepository(),
            new FakeProductRepository(),
            orderRepository,
            new FakeUnitOfWork()
        );

        // Act
        var result = await service.GetAllAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        var response = Assert.Single(result);

        Assert.Equal(userOrder.Id, response.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenOrderBelongsToUser_ReturnsOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var order = Order.Create(
            userId,
            new[]
            {
                new OrderLine(
                    Guid.NewGuid(),
                    "PROD-001",
                    "Keyboard",
                    50m,
                    2
                )
            },
            DateTime.UtcNow
        );

        var orderRepository =
            new FakeOrderRepository();

        await orderRepository.AddAsync(order);

        var service = new OrderService(
            new FakeCartRepository(),
            new FakeProductRepository(),
            orderRepository,
            new FakeUnitOfWork()
        );

        // Act
        var response = await service.GetByIdAsync(
            userId,
            order.Id,
            CancellationToken.None
        );

        // Assert
        Assert.Equal(order.Id, response.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Single(response.Items);
        Assert.Equal(100m, response.Total);
    }

    [Fact]
    public async Task CheckoutAsync_WhenStockChangesAfterAddingItemToCart_ThrowsInsufficientStockException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var product = CreateProduct(
            code: "PROD-001",
            price: 40m,
            stock: 5
        );

        var cart = new Cart(userId);

        /*
        * Cuando se agregó al carrito había stock suficiente:
        * carrito = 4
        * stock = 5
        */
        cart.AddItem(
            product.Id,
            quantity: 4
        );

        /*
        * Simula que otra compra consumió dos unidades
        * antes de que este usuario confirmara el checkout.
        *
        * Stock actual: 3
        * Cantidad solicitada: 4
        */
        product.DecreaseStock(quantity: 2);

        var cartRepository =
            new FakeCartRepository(cart);

        var orderRepository =
            new FakeOrderRepository();

        var unitOfWork =
            new FakeUnitOfWork();

        var service = new OrderService(
            cartRepository,
            new FakeProductRepository(product),
            orderRepository,
            unitOfWork
        );

        // Act
        var action = () => service.CheckoutAsync(
            userId,
            CancellationToken.None
        );

        // Assert
        await Assert.ThrowsAsync<
            InsufficientStockException
        >(action);

        // El checkout fallido no debe disminuir más el stock.
        Assert.Equal(3, product.Stock);

        // El producto debe permanecer en el carrito.
        var cartItem = Assert.Single(cart.Items);

        Assert.Equal(product.Id, cartItem.ProductId);
        Assert.Equal(4, cartItem.Quantity);

        // No debe crearse ninguna orden.
        Assert.Empty(orderRepository.Orders);
        Assert.Equal(0, orderRepository.AddCalls);

        // La operación debe terminar mediante rollback.
        Assert.Equal(1, unitOfWork.ExecuteCalls);
        Assert.Equal(0, unitOfWork.CommitCalls);
        Assert.Equal(1, unitOfWork.RollbackCalls);

        // El repositorio del carrito no debe guardar directamente.
        Assert.Equal(0, cartRepository.SaveChangesCalls);
    }

    private static Product CreateProduct(
        string code,
        decimal price,
        int stock)
    {
        return new Product(
            code: code,
            name: $"Product {code}",
            category: "Test",
            price: price,
            stock: stock
        );
    }

    private sealed class FakeCartRepository(
        Cart? initialCart = null)
        : ICartRepository
    {
        public Cart? Cart { get; private set; } =
            initialCart;

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
        private readonly IReadOnlyList<Product> _products =
            products;

        public Task<IReadOnlyList<Product>> GetAllAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_products);
        }

        public Task<IReadOnlyList<Product>>
        GetAllForAdministrationAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Product> result = _products.ToList();

            return Task.FromResult(result);
        }

        public Task<Product?> GetByIdAsync(
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
            var matchingProducts = _products
                .Where(product =>
                    ids.Contains(product.Id)
                )
                .ToList();

            return Task.FromResult<
                IReadOnlyList<Product>
            >(matchingProducts);
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
            throw new NotSupportedException(
                "Order tests do not support adding products."
            );
        }
        
        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeOrderRepository
        : IOrderRepository
    {
        private readonly List<Order> _orders = [];

        public IReadOnlyCollection<Order> Orders =>
            _orders.AsReadOnly();

        public int AddCalls { get; private set; }

        public Task AddAsync(
            Order order,
            CancellationToken cancellationToken = default)
        {
            _orders.Add(order);
            AddCalls++;

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Order>>
            GetByUserIdAsync(
                Guid userId,
                CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Order> orders = _orders
                .Where(order =>
                    order.UserId == userId
                )
                .ToList();

            return Task.FromResult(orders);
        }

        public Task<Order?> GetByIdAsync(
            Guid orderId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var order = _orders.FirstOrDefault(
                currentOrder =>
                    currentOrder.Id == orderId &&
                    currentOrder.UserId == userId
            );

            return Task.FromResult(order);
        }
    }

    private sealed class FakeUnitOfWork
        : IUnitOfWork
    {
        public int ExecuteCalls { get; private set; }

        public int CommitCalls { get; private set; }

        public int RollbackCalls { get; private set; }

        public async Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            ExecuteCalls++;

            try
            {
                var result = await operation(
                    cancellationToken
                );

                CommitCalls++;

                return result;
            }
            catch
            {
                RollbackCalls++;

                throw;
            }
        }
    }
}