using ShoppingCart.Domain.Carts;

namespace ShoppingCart.Application.Carts;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        Cart cart,
        CancellationToken cancellationToken = default
    );

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}