using ShoppingCart.Application.Carts;
using ShoppingCart.Application.Common.Exceptions;
using ShoppingCart.Application.Common.Persistence;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Orders;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.Application.Orders;

public sealed class OrderService(
    ICartRepository cartRepository,
    IProductRepository productRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork)
    : IOrderService
{
    
    public Task<OrderResponse> CheckoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        return unitOfWork.ExecuteInTransactionAsync(
            async transactionCancellationToken =>
            {
                var cart =
                    await cartRepository.GetByUserIdAsync(
                        userId,
                        transactionCancellationToken
                    );

                if (cart is null || cart.Items.Count == 0)
                {
                    throw new BusinessConflictException(
                        "The cart is empty."
                    );
                }

                var productIds = cart.Items
                    .Select(item => item.ProductId)
                    .ToArray();

                var products =
                    await productRepository
                        .GetByIdsForUpdateAsync(
                            productIds,
                            transactionCancellationToken
                        );

                EnsureAllProductsExist(
                    cart,
                    products
                );

                var productsById = products.ToDictionary(
                    product => product.Id
                );

                ValidateAllStock(
                    cart,
                    productsById
                );

                var orderLines = BuildOrderLines(
                    cart,
                    productsById
                );

                var order = Order.Create(
                    userId,
                    orderLines,
                    DateTime.UtcNow
                );

                foreach (var cartItem in cart.Items)
                {
                    var product =
                        productsById[cartItem.ProductId];

                    product.DecreaseStock(
                        cartItem.Quantity
                    );
                }

                await orderRepository.AddAsync(
                    order,
                    transactionCancellationToken
                );

                cart.Clear();

                return MapOrder(order);
            },
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<OrderResponse>>
        GetAllAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var orders =
            await orderRepository.GetByUserIdAsync(
                userId,
                cancellationToken
            );

        return orders
            .Select(MapOrder)
            .ToList();
    }

    public async Task<OrderResponse> GetByIdAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateOrderId(orderId);

        var order = await orderRepository.GetByIdAsync(
            orderId,
            userId,
            cancellationToken
        );

        if (order is null)
        {
            throw new ResourceNotFoundException(
                "Order",
                orderId
            );
        }

        return MapOrder(order);
    }

    private static void EnsureAllProductsExist(
        Cart cart,
        IReadOnlyCollection<Product> products)
    {
        var existingProductIds = products
            .Select(product => product.Id)
            .ToHashSet();

        var missingProductId = cart.Items
            .Select(item => item.ProductId)
            .FirstOrDefault(productId =>
                !existingProductIds.Contains(productId)
            );

        if (missingProductId != Guid.Empty)
        {
            throw new ResourceNotFoundException(
                "Product",
                missingProductId
            );
        }
    }

    private static void ValidateAllStock(
        Cart cart,
        IReadOnlyDictionary<Guid, Product> products)
    {
        foreach (var cartItem in cart.Items)
        {
            var product =
                products[cartItem.ProductId];

            if (cartItem.Quantity > product.Stock)
            {
                throw new InsufficientStockException(
                    product.Id,
                    cartItem.Quantity,
                    product.Stock
                );
            }
        }
    }

    private static IReadOnlyCollection<OrderLine>
        BuildOrderLines(
            Cart cart,
            IReadOnlyDictionary<Guid, Product> products)
    {
        return cart.Items
            .Select(cartItem =>
            {
                var product =
                    products[cartItem.ProductId];

                return new OrderLine(
                    ProductId: product.Id,
                    ProductCode: product.Code,
                    ProductName: product.Name,
                    UnitPrice: product.Price,
                    Quantity: cartItem.Quantity
                );
            })
            .ToList();
    }

    private static OrderResponse MapOrder(
        Order order)
    {
        var items = order.Items
            .Select(item =>
                new OrderItemResponse(
                    ProductId: item.ProductId,
                    ProductCode: item.ProductCode,
                    ProductName: item.ProductName,
                    UnitPrice: item.UnitPrice,
                    Quantity: item.Quantity,
                    Subtotal: item.Subtotal
                )
            )
            .ToList();

        return new OrderResponse(
            Id: order.Id,
            CreatedAtUtc: order.CreatedAtUtc,
            Items: items,
            Subtotal: order.Subtotal,
            Discount: order.Discount,
            Total: order.Total
        );
    }

    private static void ValidateUserId(
        Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId)
            );
        }
    }

    private static void ValidateOrderId(
        Guid orderId)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException(
                "Order identifier is required.",
                nameof(orderId)
            );
        }
    }
}