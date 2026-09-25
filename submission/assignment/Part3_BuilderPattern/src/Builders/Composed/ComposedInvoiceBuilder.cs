using InvoiceBuilding.Models;

namespace InvoiceBuilding.Builders.Composed;

/// <summary>
/// Task 3.3 — the parent builder. It owns only the invoice-level fields (id and customer)
/// and delegates each group to its own small builder:
///   AddressBuilder -> billing address and shipping address (same class, reused)
///   OrderBuilder   -> order / payment information
/// It knows nothing about street/city/zip rules or money rules.
/// </summary>
public class ComposedInvoiceBuilder
{
    private string? _invoiceId;
    private string? _customerName;
    private string? _customerEmail;
    private string? _customerPhone;
    private Address? _billingAddress;
    private Address? _shippingAddress;
    private OrderDetails? _order;

    public ComposedInvoiceBuilder WithInvoiceId(string invoiceId) { _invoiceId = invoiceId; return this; }

    public ComposedInvoiceBuilder ForCustomer(string name, string email, string? phone = null)
    {
        _customerName = name;
        _customerEmail = email;
        _customerPhone = phone;
        return this;
    }

    /// <summary>Configure the billing address with an AddressBuilder. It is validated immediately.</summary>
    public ComposedInvoiceBuilder WithBillingAddress(Action<AddressBuilder> configure)
    {
        _billingAddress = BuildAddress("Billing address", configure);
        return this;
    }

    /// <summary>Optional. When omitted, the shipping address is the same as the billing address.</summary>
    public ComposedInvoiceBuilder WithShippingAddress(Action<AddressBuilder> configure)
    {
        _shippingAddress = BuildAddress("Shipping address", configure);
        return this;
    }

    public ComposedInvoiceBuilder WithOrder(Action<OrderBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new OrderBuilder();
        configure(builder);
        _order = builder.Build();
        return this;
    }

    public Invoice Build()
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(_invoiceId)) errors.Add("InvoiceId is required.");
        if (string.IsNullOrWhiteSpace(_customerName)) errors.Add("CustomerName is required.");
        if (string.IsNullOrWhiteSpace(_customerEmail)) errors.Add("CustomerEmail is required.");
        else if (!_customerEmail.Contains('@')) errors.Add("CustomerEmail is not a valid email.");
        if (_billingAddress is null) errors.Add("Billing address is required (call WithBillingAddress).");
        if (_order is null) errors.Add("Order details are required (call WithOrder).");

        if (errors.Count > 0)
            throw new InvalidOperationException("Cannot build invoice:" + Environment.NewLine + " - " +
                                                string.Join(Environment.NewLine + " - ", errors));

        return new Invoice(_invoiceId!.Trim(), _customerName!.Trim(), _customerEmail!.Trim(),
                           string.IsNullOrWhiteSpace(_customerPhone) ? null : _customerPhone.Trim(),
                           _billingAddress!, _shippingAddress ?? _billingAddress!, _order!);
    }

    private static Address BuildAddress(string label, Action<AddressBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new AddressBuilder(label);
        configure(builder);
        return builder.Build();
    }
}
