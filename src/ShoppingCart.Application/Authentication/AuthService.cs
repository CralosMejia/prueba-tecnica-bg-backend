namespace ShoppingCart.Application.Authentication;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenGenerator tokenGenerator)
    : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByEmailAsync(
            request.Email,
            cancellationToken
        );

        if (user is null)
        {
            return null;
        }

        var isPasswordValid = passwordHasher.Verify(
            user.PasswordHash,
            request.Password
        );

        if (!isPasswordValid)
        {
            return null;
        }

        var generatedToken = tokenGenerator.Generate(user);

        return new LoginResponse(
            generatedToken.AccessToken,
            generatedToken.ExpiresAtUtc,
            user.Email,
            user.Role
        );
    }
}