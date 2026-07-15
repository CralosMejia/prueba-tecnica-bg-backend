using Microsoft.EntityFrameworkCore;
using ShoppingCart.Application.Authentication;
using ShoppingCart.Domain.Users;
using ShoppingCart.Infrastructure.Persistence;

namespace ShoppingCart.Infrastructure.Authentication;

public sealed class UserRepository(
    ShoppingCartDbContext dbContext)
    : IUserRepository
{
    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = User.NormalizeEmail(email);

        return dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                user => user.Email == normalizedEmail,
                cancellationToken
            );
    }
}