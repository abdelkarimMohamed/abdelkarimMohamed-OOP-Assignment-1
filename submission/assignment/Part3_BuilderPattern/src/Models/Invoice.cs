namespace InvoiceBuilding.Models;

public enum PaymentMethod
{
    CreditCard,
    DebitCard,
    BankTransfer,
    Cash,
    Wallet
}

/// <summary>
/// An immutable postal address. Used twice by an invoice (billing and shipping).
/// Created only by builders, which guarantee it is complete.
/// </summary>
public sealed class Address
{
    public string Street { get; }
    public string City { get; }
    public string? State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    internal Address(string street, string city, string? state, string zipCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
    }

    public override string ToString() =>
        State is null
            ? $"{Street}, {City} {ZipCode}, {Country}"
            : $"{Street}, {City}, {State} {ZipCode}, {Country}";
}

/// <summary>
/// Immutable order and payment information.
/// TotalAmount is computed, so it can never disagree with the other amounts.
/// </summary>
public sealed class OrderDetails
{
    public DateOnly OrderDate { get; }
    public PaymentMethod PaymentMethod { get; }
    public string Currency { get; }
    public decimal SubTotal { get; }
    public decimal DiscountAmount { get; }
    public decimal TaxAmount { get; }
    public decimal TotalAmount => SubTotal - DiscountAmount + TaxAmount;

    internal OrderDetails(DateOnly orderDate, PaymentMethod paymentMethod, string currency,
                          decimal subTotal, decimal discountAmount, decimal taxAmount)
    {
        OrderDate = orderDate;
        PaymentMethod = paymentMethod;
        Currency = currency;
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
    }
}

/// <summary>
/// The final invoice. All properties are get-only; the constructor is internal,
/// so the only way to create an Invoice is through a builder.
/// </summary>
public sealed class Invoice
{
    public string InvoiceId { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public string? CustomerPhone { get; }
    public Address BillingAddress { get; }
    public Address ShippingAddress { get; }
    public OrderDetails Order { get; }

    public decimal TotalAmount => Order.TotalAmount;

    internal Invoice(string invoiceId, string customerName, string customerEmail, string? customerPhone,
                     Address billingAddress, Address shippingAddress, OrderDetails order)
    {
        InvoiceId = invoiceId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;
        BillingAddress = billingAddress ?? throw new ArgumentNullException(nameof(billingAddress));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Order = order ?? throw new ArgumentNullException(nameof(order));
    }
}
