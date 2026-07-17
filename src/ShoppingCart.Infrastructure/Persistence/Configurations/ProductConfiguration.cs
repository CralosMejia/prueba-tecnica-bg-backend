using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShoppingCart.Domain.Products;


namespace ShoppingCart.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Price).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.Stock).IsRequired();
        builder.Property(p => p.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasData(
            new
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Code = "PRD-001",
                Name = "Mechanical Keyboard",
                Category = "Technology",
                Price = 80m,
                Stock = 15,
                IsActive = true
            },
            new
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Code = "PRD-002",
                Name = "Wireless Mouse",
                Category = "Technology",
                Price = 35m,
                Stock = 25,
                IsActive = true
            },
            new
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Code = "PRD-003",
                Name = "Monitor 24 Inches",
                Category = "Monitors",
                Price = 180m,
                Stock = 8,
                IsActive = true
            },
            new
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Code = "PRD-004",
                Name = "USB Webcam",
                Category = "Accessories",
                Price = 55m,
                Stock = 12,
                IsActive = true
            },
            new
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Code = "PRD-005",
                Name = "Laptop",
                Category = "Computers",
                Price = 850m,
                Stock = 5,
                IsActive = true
            },
            new
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Code = "PRD-006",
                Name = "Gaming Headset",
                Category = "Audio",
                Price = 95m,
                Stock = 0,
                IsActive = true
            }
        );
    }
}