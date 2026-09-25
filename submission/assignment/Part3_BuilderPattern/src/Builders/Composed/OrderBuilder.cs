using InvoiceBuilding.Models;

namespace InvoiceBuilding.Builders.Composed;

/// <summary>
/// Task 3.3 — builds the order / payment part of an invoice. Owns every money rule, and nothing else.
/// Mandatory: OrderDate, PaymentMethod, Currency, SubTotal.
/// Optional:  Discount (default 0), Tax (default 0). TotalAmount is always computed.
/// </summary>
public class OrderBuilder
{
    private DateOnly? _orderDate;
    private PaymentMethod? _paymentMethod;
    private string? _currency;
    private decimal? _subTotal;
    private decimal _discount;
    private decimal _tax;

    public OrderBuilder WithOrderDate(DateOnly date) { _orderDate = date; return this; }
    public OrderBuilder WithPaymentMethod(PaymentMethod method) { _paymentMethod = method; return this; }
    public OrderBuilder WithCurrency(string currency) { _currency = currency; return this; }
    public OrderBuilder WithSubTotal(decimal subTotal) { _subTotal = subTotal; return this; }
    public OrderBuilder WithDiscount(decimal discount) { _discount = discount; return this; }
    public OrderBuilder WithTax(decimal tax) { _tax = tax; return this; }

    public OrderDetails Build()
    {
        var errors = new List<string>();
        if (_orderDate is null) errors.Add("OrderDate is required.");
        if (_paymentMethod is null) errors.Add("PaymentMethod is required.");
        if (string.IsNullOrWhiteSpace(_currency)) errors.Add("Currency is required.");
        else if (_currency.Trim().Length != 3) errors.Add("Currency must be a 3-letter code (e.g. EGP).");
        if (_subTotal is null) errors.Add("SubTotal is required.");
        else if (_subTotal < 0) errors.Add("SubTotal cannot be negative.");
        if (_discount < 0) errors.Add("Discount cannot be negative.");
        if (_tax < 0) errors.Add("Tax cannot be negative.");
        if (_subTotal is not null && _discount > _subTotal) errors.Add("Discount cannot exceed SubTotal.");

        if (errors.Count > 0)
            throw new InvalidOperationException("Order details are invalid: " + string.Join(" ", errors));

        return new OrderDetails(_orderDate!.Value, _paymentMethod!.Value, _currency!.Trim().ToUpperInvariant(),
                                _subTotal!.Value, _discount, _tax);
    }
}
