using Microsoft.AspNetCore.Identity;
using ShoppingCart.Application.Authentication;

namespace ShoppingCart.Infrastructure.Authentication;

public sealed class AspNetPasswordHasher
    : IPasswordHasher
{
    private static readonly object UserContext = new();

    private readonly PasswordHasher<object> _passwordHasher =
        new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(
            UserContext,
            password
        );
    }

    public bool Verify(
        string passwordHash,
        string providedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            passwordHash
        );

        ArgumentException.ThrowIfNullOrWhiteSpace(
            providedPassword
        );

        var result = _passwordHasher.VerifyHashedPassword(
            UserContext,
            passwordHash,
            providedPassword
        );

        return result is
            PasswordVerificationResult.Success or
            PasswordVerificationResult.SuccessRehashNeeded;
    }
}