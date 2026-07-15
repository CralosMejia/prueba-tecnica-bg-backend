namespace ShoppingCart.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    public Product(string code, string name, string category, decimal price, int stock)
    {
        Validate(price, stock);
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Category = category;
        Price = price;
        Stock = stock;
    }

    private void Validate(decimal price, int stock)
    {

        if (price < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive.");
        }

        if (stock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock must be positive.");
        }
    }
}