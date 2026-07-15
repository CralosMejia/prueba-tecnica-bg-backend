using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;


using ShoppingCart.Application.Products;
using ShoppingCart.Infrastructure.Products;
using ShoppingCart.Infrastructure.Authentication;
using ShoppingCart.Application.Authentication;
using ShoppingCart.Application.Carts;
using ShoppingCart.Infrastructure.Carts;

namespace ShoppingCart.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ShoppingCartDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                }
            );
        });

        // Repositorios
        services.AddScoped<
            IProductRepository,
            ProductRepository
        >();

        services.AddScoped<
            IUserRepository,
            UserRepository
        >();

        services.AddScoped<
            ICartRepository,
            CartRepository
        >();

        // Seguridad de contraseñas
        services.AddScoped<
            IPasswordHasher,
            AspNetPasswordHasher
        >();

        // Generación de tokens JWT
        services.AddScoped<
            ITokenGenerator,
            JwtTokenGenerator
        >();

        // Vincula la configuración Jwt con JwtOptions
        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName
            )
        );

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración 'Jwt:Key'."
            );

        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración 'Jwt:Issuer'."
            );

        var jwtAudience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "No se encontró la configuración 'Jwt:Audience'."
            );

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
                JwtBearerDefaults.AuthenticationScheme;

            options.DefaultChallengeScheme =
                JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtKey)
                        ),

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    NameClaimType =
                        ClaimTypes.NameIdentifier,

                    RoleClaimType =
                        ClaimTypes.Role
                };
        });

        services.AddAuthorization();

        return services;
    }
}