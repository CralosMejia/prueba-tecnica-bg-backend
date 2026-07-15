using ShoppingCart.Domain.Users;

namespace ShoppingCart.Application.Authentication;

public interface ITokenGenerator
{
    GeneratedToken Generate(User user);
}