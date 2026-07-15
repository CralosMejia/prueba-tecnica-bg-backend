using Microsoft.EntityFrameworkCore;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.Infrastructure.Persistence;

public class ShoppingCartDbContext : DbContext
{
    public ShoppingCartDbContext(
        DbContextOptions<ShoppingCartDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ShoppingCartDbContext).Assembly
        );

        base.OnModelCreating(modelBuilder);
    }
}