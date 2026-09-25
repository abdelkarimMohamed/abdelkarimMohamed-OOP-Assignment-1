namespace OrderSystem.Models;

/// <summary>
/// An order placed by a customer. The order owns its lines and its paid state,
/// and enforces its own rules (no changes after payment, no paying an empty order).
/// </summary>
public class Order
{
    private readonly List<OrderLine> _lines = new();

    public int Id { get; }
    public Customer Customer { get; }
    public DateOnly Date { get; }
    public bool IsPaid { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();

    public Order(int id, Customer customer, DateOnly date)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Order id must be positive.");

        Id = id;
        Customer = customer ?? throw new ArgumentNullException(nameof(customer));
        Date = date;
    }

    public void AddLine(Product product, int quantity)
    {
        if (IsPaid)
            throw new InvalidOperationException("Cannot change a paid order.");
        ArgumentNullException.ThrowIfNull(product);

        // Same behavior as the original program: stock is taken when the line is added.
        product.RemoveStock(quantity);
        _lines.Add(new OrderLine(product, quantity));
    }

    public void MarkPaid()
    {
        if (IsPaid)
            throw new InvalidOperationException($"Order #{Id} is already paid.");
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot pay an empty order.");

        IsPaid = true;
    }

    public decimal Subtotal => _lines.Sum(line => line.LineTotal);
    public decimal Discount => Subtotal * Customer.DiscountRate;
    public decimal Total => Subtotal - Discount;
}
