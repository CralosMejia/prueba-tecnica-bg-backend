using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Application.Products;

public sealed record UpdateProductRequest
{
    
    [MaxLength(100)]
    public  string? Name { get; set;}

    [MaxLength(50)]
    public  string?  Category { get; set; }

    [Range(0.01, 10000)]
    public  decimal? Price { get; set; }

    [Range(0, int.MaxValue)]
    public  int? Stock { get; set; }
}
