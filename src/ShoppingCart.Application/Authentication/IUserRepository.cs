using ShoppingCart.Domain.Users;

namespace ShoppingCart.Application.Authentication;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default
    );
}