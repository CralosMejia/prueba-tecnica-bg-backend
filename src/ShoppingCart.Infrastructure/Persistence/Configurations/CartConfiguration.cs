using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class CartConfiguration
    : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("carts");

        builder.HasKey(cart => cart.Id);

        builder.Property(cart => cart.UserId)
            .IsRequired();

        // Garantiza un solo carrito por usuario.
        builder.HasIndex(cart => cart.UserId)
            .IsUnique();

        // Relación User 1:1 Cart.
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relación Cart 1:N CartItem.
        builder.HasMany(cart => cart.Items)
            .WithOne()
            .HasForeignKey(item => item.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF Core utilizará directamente el campo privado _items.
        builder.Navigation(cart => cart.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}