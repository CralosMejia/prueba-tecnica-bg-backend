using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


using ShoppingCart.Infrastructure.Persistence;
using ShoppingCart.Application.Products;
using ShoppingCart.Infrastructure.Products;

namespace ShoppingCart.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
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

        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}