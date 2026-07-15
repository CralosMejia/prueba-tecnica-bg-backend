using System.Security.Claims;

namespace ShoppingCart.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetRequiredUserId(
        this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        var userIdValue = principal
            .FindFirst(ClaimTypes.NameIdentifier)
            ?.Value;

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedAccessException(
                "The authenticated user identifier is invalid."
            );
        }

        return userId;
    }
}