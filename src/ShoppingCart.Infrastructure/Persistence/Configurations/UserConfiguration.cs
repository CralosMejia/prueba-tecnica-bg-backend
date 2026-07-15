using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Users;

namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration
    : IEntityTypeConfiguration<User>
{
    public void Configure(
        EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(user => user.Email)
            .IsUnique();

        builder.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(user => user.Role)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasData(
            new
            {
                Id = Guid.Parse(
                    "11111111-1111-1111-1111-111111111111"
                ),
                Email = "customer@shoppingcart.com",
                //Customer123!
                PasswordHash =
                    "AQAAAAIAAYagAAAAEPOxdB34UwagFKUgkIkXgQsot0S5P7e8NjUAEJm1JWi2XnLEhOG2hRVpZLzyjYdMqA==",
                Role = UserRoles.Customer
            },
            new
            {
                Id = Guid.Parse(
                    "22222222-2222-2222-2222-222222222222"
                ),
                Email = "admin@shoppingcart.com",
                //Admin123!
                PasswordHash =
                    "AQAAAAIAAYagAAAAEA/Ba/aYaifQlnE3UhFCCrzC96HRonJiOV4iHNeWk+1/C4+y/6EosmK3XQZyxYjmyQ==",
                Role = UserRoles.Admin
            }
        );

    }
}