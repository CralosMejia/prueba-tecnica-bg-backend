using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Carts;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Infrastructure.Persistence;

namespace ShoppingCart.Infrastructure.Carts;

public sealed class CartRepository(
    ShoppingCartDbContext dbContext)
    : ICartRepository
{
    public Task<Cart?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Carts
            .Include(cart => cart.Items)
            .SingleOrDefaultAsync(
                cart => cart.UserId == userId,
                cancellationToken
            );
    }

    public async Task AddAsync(
        Cart cart,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Carts.AddAsync(
            cart,
            cancellationToken
        );
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(
            cancellationToken
        );
    }
}