namespace ShoppingCart.Application.Authentication;

public sealed record GeneratedToken(
    string AccessToken,
    DateTime ExpiresAtUtc
);