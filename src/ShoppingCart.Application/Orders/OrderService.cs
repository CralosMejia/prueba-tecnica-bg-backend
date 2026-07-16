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
    public async Task<OrderResponse> CheckoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken
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
            await productRepository.GetByIdsForUpdateAsync(
                productIds,
                cancellationToken
            );

        EnsureAllProductsExist(
            cart,
            products
        );

        var productsById = products.ToDictionary(
            product => product.Id
        );

        /*
         * Primero validamos todos los productos.
         * Todavía no modificamos stock, carrito ni órdenes.
         */
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

        /*
         * Solo después de que todas las validaciones pasaron
         * comenzamos a modificar el estado.
         */
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
            cancellationToken
        );

        cart.Clear();

        /*
         * Único punto de confirmación:
         * - Order y OrderItems
         * - Product.Stock
         * - eliminación de CartItems
         */
        await unitOfWork.SaveChangesAsync(
            cancellationToken
        );

        return MapOrder(order);
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