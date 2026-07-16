using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Orders;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class OrderConfiguration
    : IEntityTypeConfiguration<Order>
{
    public void Configure(
        EntityTypeBuilder<Order> builder)
    {
        builder.ToTable(
            "orders",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_orders_subtotal_non_negative",
                    "`Subtotal` >= 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_orders_discount_non_negative",
                    "`Discount` >= 0"
                );

                tableBuilder.HasCheckConstraint(
                    "CK_orders_total_non_negative",
                    "`Total` >= 0"
                );
            }
        );

        builder.HasKey(order => order.Id);

        builder.Property(order => order.UserId)
            .IsRequired();

        builder.Property(order => order.CreatedAtUtc)
            .IsRequired();

        builder.Property(order => order.Subtotal)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.Discount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(order => order.Total)
            .HasPrecision(18, 2)
            .IsRequired();

        // Un usuario puede tener múltiples órdenes.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(order => order.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Una orden contiene múltiples detalles.
        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey(item => item.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF Core utiliza el campo privado _items.
        builder.Navigation(order => order.Items)
            .UsePropertyAccessMode(
                PropertyAccessMode.Field
            );

        // Optimiza la consulta del historial del usuario.
        builder.HasIndex(order => new
        {
            order.UserId,
            order.CreatedAtUtc
        });
    }
}