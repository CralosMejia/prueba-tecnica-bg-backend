using Microsoft.EntityFrameworkCore;
using ShoppingCart.Infrastructure.Persistence;
using Testcontainers.MySql;

namespace ShoppingCart.IntegrationTests.Infrastructure;

public sealed class MySqlDatabaseFixture
    : IAsyncLifetime
{
    private readonly MySqlContainer _container =
    new MySqlBuilder("mysql:8.4")
        .WithDatabase(
            "shopping_cart_integration_tests"
        )
        .WithUsername(
            "integration_user"
        )
        .WithPassword(
            "IntegrationPassword123!"
        )
        .WithCommand(
            "--log-bin-trust-function-creators=1"
        )
        .Build();

    public ShoppingCartDbContext CreateDbContext()
    {
        var connectionString =
            _container.GetConnectionString();

        var options =
            new DbContextOptionsBuilder<
                ShoppingCartDbContext
            >()
            .UseMySql(
                connectionString,
                ServerVersion.AutoDetect(
                    connectionString
                ),
                mysqlOptions =>
                {
                    mysqlOptions.EnableRetryOnFailure();
                }
            )
            .Options;

        return new ShoppingCartDbContext(
            options
        );
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using var dbContext =
            CreateDbContext();

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}