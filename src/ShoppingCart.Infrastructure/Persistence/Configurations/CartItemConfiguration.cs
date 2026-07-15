using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Carts;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration
    : IEntityTypeConfiguration<CartItem>
{
    public void Configure(
        EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(
            "cart_items",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_cart_items_quantity_positive",
                    "`Quantity` > 0"
                );
            }
        );

        // Un producto solo puede aparecer una vez por carrito.
        builder.HasKey(item => new
        {
            item.CartId,
            item.ProductId
        });

        builder.Property(item => item.Quantity)
            .IsRequired();

        // Relación Product 1:N CartItem.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // Facilita búsquedas de carritos que contienen un producto.
        builder.HasIndex(item => item.ProductId);
    }
}