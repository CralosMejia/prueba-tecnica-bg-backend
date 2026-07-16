using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Orders;
using ShoppingCart.Domain.Products;

namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(
        EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable(
            "order_items",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_order_items_quantity_positive",
                    "`Quantity` > 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_order_items_unit_price_non_negative",
                    "`UnitPrice` >= 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_order_items_subtotal_non_negative",
                    "`Subtotal` >= 0"
                );
            }
        );

        builder.HasKey(item => item.Id);

        builder.Property(item => item.OrderId)
            .IsRequired();

        builder.Property(item => item.ProductId)
            .IsRequired();

        builder.Property(item => item.ProductCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(item => item.ProductName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(item => item.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.Property(item => item.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        // Mantiene trazabilidad hacia el producto original.
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(item => item.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.OrderId);

        builder.HasIndex(item => item.ProductId);
    }
}