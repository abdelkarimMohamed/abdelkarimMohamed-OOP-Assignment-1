namespace OrderSystem.Models;

/// <summary>
/// A customer of the store. Owns its own identity data and knows
/// which discount it is entitled to (replaces the VIP check that was
/// buried inside the order-total function).
/// </summary>
public class Customer
{
    private const decimal VipDiscountRate = 0.10m;

    public int Id { get; }
    public string Name { get; }
    public string Email { get; }
    public string City { get; }
    public bool IsVip { get; }

    public Customer(int id, string name, string email, string city, bool isVip)
    {
        if (id <= 0)
            throw new ArgumentOutOfRangeException(nameof(id), "Customer id must be positive.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Customer email is required.", nameof(email));

        Id = id;
        Name = name;
        Email = email;
        City = city ?? string.Empty;
        IsVip = isVip;
    }

    /// <summary>The discount this customer gets on an order (0.10 for VIP, 0 otherwise).</summary>
    public decimal DiscountRate => IsVip ? VipDiscountRate : 0m;
}
