using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Orders;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Products;
using ShoppingCart.Infrastructure.Carts;
using ShoppingCart.Infrastructure.Orders;
using ShoppingCart.Infrastructure.Persistence;
using ShoppingCart.Infrastructure.Products;
using ShoppingCart.IntegrationTests.Infrastructure;

namespace ShoppingCart.IntegrationTests.Orders;

public sealed class OrderTransactionTests
    : IClassFixture<MySqlDatabaseFixture>
{
    private const string FailureTriggerName =
        "trg_fail_order_item_insert";

    private readonly MySqlDatabaseFixture _fixture;

    public OrderTransactionTests(
        MySqlDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CheckoutAsync_WhenOrderItemInsertFails_RollsBackOrderStockAndCart()
    {
        // Arrange
        Guid userId;
        Guid productId;

        const int initialStock = 5;
        const int cartQuantity = 2;

        /*
         * Preparamos datos reales en MySQL.
         */
        await using (
            var setupContext =
                _fixture.CreateDbContext()
        )
        {
            var user = await setupContext.Users
                .SingleAsync(currentUser =>
                    currentUser.Email ==
                    "customer@shoppingcart.com"
                );

            var product = new Product(
                code:
                    $"INT-{Guid.NewGuid():N}",
                name:
                    "Integration Rollback Product",
                category:
                    "Integration Tests",
                price: 30m,
                stock: initialStock
            );

            var cart = new Cart(user.Id);

            cart.AddItem(
                product.Id,
                cartQuantity
            );

            setupContext.Products.Add(
                product
            );

            setupContext.Carts.Add(
                cart
            );

            await setupContext.SaveChangesAsync();

            userId = user.Id;
            productId = product.Id;
        }

        /*
         * Este trigger provoca deliberadamente un error
         * al insertar un OrderItem.
         *
         * La cabecera de la orden debe insertarse antes
         * que sus detalles por la clave foránea, por lo
         * que el fallo ocurre después de que la
         * transacción ya ejecutó una escritura real.
         */
        await CreateFailureTriggerAsync();

        try
        {
            await using var checkoutContext =
                _fixture.CreateDbContext();

            IOrderService service =
                new OrderService(
                    new CartRepository(
                        checkoutContext
                    ),
                    new ProductRepository(
                        checkoutContext
                    ),
                    new OrderRepository(
                        checkoutContext
                    ),
                    new EfUnitOfWork(
                        checkoutContext
                    )
                );

            // Act
            var action = () =>
                service.CheckoutAsync(
                    userId,
                    CancellationToken.None
                );

            // Assert
            await Assert.ThrowsAsync<
                DbUpdateException
            >(action);
        }
        finally
        {
            await DropFailureTriggerAsync();
        }

        /*
         * Usamos un contexto nuevo para evitar leer
         * entidades del ChangeTracker utilizado durante
         * la operación fallida.
         */
        await using var verificationContext =
            _fixture.CreateDbContext();

        var persistedProduct =
            await verificationContext.Products
                .AsNoTracking()
                .SingleAsync(product =>
                    product.Id == productId
                );

        var persistedCart =
            await verificationContext.Carts
                .AsNoTracking()
                .Include(cart => cart.Items)
                .SingleAsync(cart =>
                    cart.UserId == userId
                );

        var orderExists =
            await verificationContext.Orders
                .AsNoTracking()
                .AnyAsync(order =>
                    order.UserId == userId
                );

        /*
         * El stock debe mantener el valor anterior.
         */
        Assert.Equal(
            initialStock,
            persistedProduct.Stock
        );

        /*
         * El carrito debe conservar el producto.
         */
        var persistedCartItem =
            Assert.Single(
                persistedCart.Items
            );

        Assert.Equal(
            productId,
            persistedCartItem.ProductId
        );

        Assert.Equal(
            cartQuantity,
            persistedCartItem.Quantity
        );

        /*
         * La cabecera de la orden también debe haberse
         * revertido.
         */
        Assert.False(orderExists);
    }

    private async Task CreateFailureTriggerAsync()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Database
            .ExecuteSqlRawAsync(
                $"""
                DROP TRIGGER IF EXISTS `{FailureTriggerName}`
                """
            );

        await dbContext.Database
            .ExecuteSqlRawAsync(
                $"""
                CREATE TRIGGER `{FailureTriggerName}`
                BEFORE INSERT ON `order_items`
                FOR EACH ROW
                SIGNAL SQLSTATE '45000'
                SET MESSAGE_TEXT =
                    'Forced rollback integration test'
                """
            );
    }

    private async Task DropFailureTriggerAsync()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.Database
            .ExecuteSqlRawAsync(
                $"""
                DROP TRIGGER IF EXISTS `{FailureTriggerName}`
                """
            );
    }
}