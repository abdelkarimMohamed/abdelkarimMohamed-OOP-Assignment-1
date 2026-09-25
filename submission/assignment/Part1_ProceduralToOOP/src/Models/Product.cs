namespace OrderSystem.Models;

/// <summary>
/// A product in the catalog. The product itself protects its stock:
/// nobody outside can make the stock negative.
/// </summary>
public class Product
{
    public int Id { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Stock { get; private set; }

    public Product(int id, string name, decimal price, int stock)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Product id must be positive.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name is required.", nameof(name));
        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative.");
        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Stock cannot be negative.");

        Id = id;
        Name = name;
        Price = price;
        Stock = stock;
    }

    public bool HasStock(int quantity) => Stock >= quantity;

    /// <summary>Takes items out of stock. Fails if there is not enough.</summary>
    public void RemoveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");
        if (!HasStock(quantity))
            throw new InvalidOperationException($"Not enough stock for product #{Id}.");

        Stock -= quantity;
    }
}
