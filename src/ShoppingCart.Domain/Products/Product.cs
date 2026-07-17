namespace ShoppingCart.Domain.Products;

public class Product
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Category { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; }

    public Product(string code, string name, string category, decimal price, int stock)
    {
        ValidateCreateProduct(price, stock);
        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        Category = category;
        Price = price;
        Stock = stock;
        IsActive = true;
    }

    private void ValidateCreateProduct(decimal price, int stock)
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

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        }

        if (Stock < quantity)
        {
            throw new InvalidOperationException("Insufficient stock.");
        }

        Stock -= quantity;
    }

    public void UpdateProduct(
        string? name,
        string? category,
        decimal? price,
        int? stock)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(
                    "Name is required.",
                    nameof(name)
                );
            }

            Name = name;
        }

        if (category is not null)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                throw new ArgumentException(
                    "Category is required.",
                    nameof(category)
                );
            }

            Category = category;
        }

        if (price.HasValue)
        {
            if (price.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(price),
                    "Price must be positive."
                );
            }

            Price = price.Value;
        }

        if (stock.HasValue)
        {
            if (stock.Value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stock),
                    "Stock must be positive."
                );
            }

            Stock = stock.Value;
        }
    }

    public void ChangeActivityStatus()
    {
        IsActive = !IsActive;
    }
}