using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShoppingCart.Application.Authentication;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.Infrastructure.Authentication;

public sealed class JwtTokenGenerator(
    IOptions<JwtOptions> jwtOptions)
    : ITokenGenerator
{
    private readonly JwtOptions _options =
        jwtOptions.Value;

    public GeneratedToken Generate(User user)
    {
        var issuedAtUtc = DateTime.UtcNow;

        var expiresAtUtc = issuedAtUtc.AddMinutes(
            _options.ExpirationMinutes
        );

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),
            new(
                ClaimTypes.Email,
                user.Email
            ),
            new(
                ClaimTypes.Role,
                user.Role
            ),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()
            )
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Key)
        );

        var signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials
        );

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return new GeneratedToken(
            accessToken,
            expiresAtUtc
        );
    }
}