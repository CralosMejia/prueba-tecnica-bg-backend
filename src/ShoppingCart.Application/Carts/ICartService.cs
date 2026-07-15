namespace ShoppingCart.Application.Carts;

public interface ICartService
{
    Task<CartResponse> GetAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<CartResponse> AddItemAsync(
        Guid userId,
        AddCartItemRequest request,
        CancellationToken cancellationToken = default
    );

    Task<CartResponse> UpdateQuantityAsync(
        Guid userId,
        Guid productId,
        UpdateCartItemRequest request,
        CancellationToken cancellationToken = default
    );

    Task<CartResponse> RemoveItemAsync(
        Guid userId,
        Guid productId,
        CancellationToken cancellationToken = default
    );

    Task<CartResponse> ClearAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}