using Microsoft.Extensions.DependencyInjection;
using ShoppingCart.Application.Products;
using ShoppingCart.Application.Authentication;

namespace ShoppingCart.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        // Servicios relacionados con productos
        services.AddScoped<
            IProductService,
            ProductService
        >();

        // Servicios relacionados con autenticación
        services.AddScoped<
            IAuthService,
            AuthService
        >();

        return services;
    }
}