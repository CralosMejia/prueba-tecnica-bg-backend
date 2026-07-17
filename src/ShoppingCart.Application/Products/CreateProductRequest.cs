using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Application.Products;

public sealed record CreateProductRequest
{
    [Required]
    [MaxLength(50)]
    public required string Code { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Category { get; set; }

    [Required]
    [Range(0.01, 10000)]
    public required decimal Price { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public required int Stock { get; set; }
}
