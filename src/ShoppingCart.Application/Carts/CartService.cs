using ShoppingCart.Application.Common.Exceptions;
using ShoppingCart.Application.Products;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Products;


namespace ShoppingCart.Application.Carts;

public sealed class CartService(
    ICartRepository cartRepository,
    IProductRepository productRepository)
    : ICartService
{
    public async Task<CartResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken
        );

        if (cart is null)
        {
            return CartResponse.Empty;
        }

        return await BuildResponseAsync(
            cart,
            cancellationToken
        );
    }

    public async Task<CartResponse> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateQuantity(request.Quantity);

        var product = await GetRequiredProductAsync(
            request.ProductId,
            cancellationToken
        );

        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken
        );

        var isNewCart = cart is null;

        cart ??= new Cart(userId);

        var currentQuantity = cart.Items
            .FirstOrDefault(
                item => item.ProductId == product.Id
            )
            ?.Quantity ?? 0;

        var requestedTotalQuantity =
            currentQuantity + request.Quantity;

        ValidateStock(
            product,
            requestedTotalQuantity
        );

        cart.AddItem(
            product.Id,
            request.Quantity
        );

        if (isNewCart)
        {
            await cartRepository.AddAsync(
                cart,
                cancellationToken
            );
        }

        await cartRepository.SaveChangesAsync(
            cancellationToken
        );

        return await BuildResponseAsync(
            cart,
            cancellationToken
        );
    }

    public async Task<CartResponse> UpdateQuantityAsync(
        Guid userId,
        Guid productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateProductId(productId);
        ArgumentNullException.ThrowIfNull(request);
        ValidateQuantity(request.Quantity);

        var cart = await GetRequiredCartAsync(
            userId,
            cancellationToken
        );

        var cartItem = cart.Items.FirstOrDefault(
            item => item.ProductId == productId
        );

        if (cartItem is null)
        {
            throw new ResourceNotFoundException(
                "Cart item",
                productId
            );
        }

        var product = await GetRequiredProductAsync(
            productId,
            cancellationToken
        );

        ValidateStock(
            product,
            request.Quantity
        );

        cart.UpdateQuantity(
            productId,
            request.Quantity
        );

        await cartRepository.SaveChangesAsync(
            cancellationToken
        );

        return await BuildResponseAsync(
            cart,
            cancellationToken
        );
    }

    public async Task<CartResponse> RemoveItemAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);
        ValidateProductId(productId);

        var cart = await GetRequiredCartAsync(
            userId,
            cancellationToken
        );

        var wasRemoved = cart.RemoveItem(productId);

        if (!wasRemoved)
        {
            throw new ResourceNotFoundException(
                "Cart item",
                productId
            );
        }

        await cartRepository.SaveChangesAsync(
            cancellationToken
        );

        return await BuildResponseAsync(
            cart,
            cancellationToken
        );
    }

    public async Task<CartResponse> ClearAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        ValidateUserId(userId);

        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken
        );

        if (cart is null)
        {
            return CartResponse.Empty;
        }

        cart.Clear();

        await cartRepository.SaveChangesAsync(
            cancellationToken
        );

        return CartResponse.Empty;
    }

    private async Task<Cart> GetRequiredCartAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cart = await cartRepository.GetByUserIdAsync(
            userId,
            cancellationToken
        );

        return cart ?? throw new ResourceNotFoundException(
            "Cart",
            userId
        );
    }

    private async Task<Product> GetRequiredProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        ValidateProductId(productId);

        var product = await productRepository.GetByIdAsync(
            productId,
            cancellationToken
        );

        return product ?? throw new ResourceNotFoundException(
            "Product",
            productId
        );
    }

    private async Task<CartResponse> BuildResponseAsync(
        Cart cart,
        CancellationToken cancellationToken)
    {
        if (cart.Items.Count == 0)
        {
            return CartResponse.Empty;
        }

        var itemResponses =
            new List<CartItemResponse>();

        var unitPrices =
            new Dictionary<Guid, decimal>();

        foreach (var item in cart.Items)
        {
            var product = await GetRequiredProductAsync(
                item.ProductId,
                cancellationToken
            );

            unitPrices[product.Id] = product.Price;

            itemResponses.Add(
                new CartItemResponse(
                    ProductId: product.Id,
                    Code: product.Code,
                    Name: product.Name,
                    UnitPrice: product.Price,
                    Quantity: item.Quantity,
                    Subtotal: item.CalculateSubtotal(
                        product.Price
                    )
                )
            );
        }

        var totals = cart.CalculateTotals(
            unitPrices
        );

        return new CartResponse(
            Items: itemResponses,
            Subtotal: totals.Subtotal,
            Discount: totals.Discount,
            Total: totals.Total
        );
    }

    private static void ValidateStock(
        Product product,
        int requestedQuantity)
    {
        if (requestedQuantity > product.Stock)
        {
            throw new InsufficientStockException(
                product.Id,
                requestedQuantity,
                product.Stock
            );
        }
    }

    private static void ValidateUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User identifier is required.",
                nameof(userId)
            );
        }
    }

    private static void ValidateProductId(
        Guid productId)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException(
                "Product identifier is required.",
                nameof(productId)
            );
        }
    }

    private static void ValidateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero."
            );
        }
    }
}