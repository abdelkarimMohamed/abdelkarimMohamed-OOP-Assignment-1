namespace OrderSystem.Models;

/// <summary>
/// One line of an order. It keeps a reference to the real Product object
/// and a snapshot of the unit price at the moment the line was added,
/// so a later price change never rewrites the history of an order.
/// </summary>
public class OrderLine
{
    public Product Product { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }

    public OrderLine(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        Product = product ?? throw new ArgumentNullException(nameof(product));
        Quantity = quantity;
        UnitPrice = product.Price;
    }

    public decimal LineTotal => UnitPrice * Quantity;
}
