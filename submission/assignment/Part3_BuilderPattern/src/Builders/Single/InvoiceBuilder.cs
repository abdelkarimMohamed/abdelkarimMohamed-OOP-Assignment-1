using InvoiceBuilding.Models;

namespace InvoiceBuilding.Builders.Single;

/// <summary>
/// Task 3.2 — ONE fluent builder for the whole invoice.
///
/// Mandatory: InvoiceId, CustomerName, CustomerEmail, full billing address
///            (street, city, zip, country), OrderDate, PaymentMethod, Currency, SubTotal.
/// Optional:  CustomerPhone, State (billing/shipping), DiscountAmount (0), TaxAmount (0),
///            shipping address (defaults to the billing address when none is given).
///
/// Build() reports EVERY missing mandatory field at once, with a clear message.
/// Notice how the billing and shipping sections are the same five fields written twice.
/// </summary>
public class InvoiceBuilder
{
    private string? _invoiceId;
    private string? _customerName;
    private string? _customerEmail;
    private string? _customerPhone;

    private string? _billingStreet, _billingCity, _billingState, _billingZipCode, _billingCountry;
    private string? _shippingStreet, _shippingCity, _shippingState, _shippingZipCode, _shippingCountry;

    private DateOnly? _orderDate;
    private PaymentMethod? _paymentMethod;
    private string? _currency;
    private decimal? _subTotal;
    private decimal _discountAmount;
    private decimal _taxAmount;

    // ---------- Customer ----------
    public InvoiceBuilder WithInvoiceId(string invoiceId) { _invoiceId = invoiceId; return this; }
    public InvoiceBuilder WithCustomerName(string name) { _customerName = name; return this; }
    public InvoiceBuilder WithCustomerEmail(string email) { _customerEmail = email; return this; }
    public InvoiceBuilder WithCustomerPhone(string phone) { _customerPhone = phone; return this; }

    // ---------- Billing address ----------
    public InvoiceBuilder WithBillingStreet(string street) { _billingStreet = street; return this; }
    public InvoiceBuilder WithBillingCity(string city) { _billingCity = city; return this; }
    public InvoiceBuilder WithBillingState(string state) { _billingState = state; return this; }
    public InvoiceBuilder WithBillingZipCode(string zip) { _billingZipCode = zip; return this; }
    public InvoiceBuilder WithBillingCountry(string country) { _billingCountry = country; return this; }

    // ---------- Shipping address (same five fields again) ----------
    public InvoiceBuilder WithShippingStreet(string street) { _shippingStreet = street; return this; }
    public InvoiceBuilder WithShippingCity(string city) { _shippingCity = city; return this; }
    public InvoiceBuilder WithShippingState(string state) { _shippingState = state; return this; }
    public InvoiceBuilder WithShippingZipCode(string zip) { _shippingZipCode = zip; return this; }
    public InvoiceBuilder WithShippingCountry(string country) { _shippingCountry = country; return this; }

    // ---------- Order / payment ----------
    public InvoiceBuilder WithOrderDate(DateOnly date) { _orderDate = date; return this; }
    public InvoiceBuilder WithPaymentMethod(PaymentMethod method) { _paymentMethod = method; return this; }
    public InvoiceBuilder WithCurrency(string currency) { _currency = currency; return this; }
    public InvoiceBuilder WithSubTotal(decimal subTotal) { _subTotal = subTotal; return this; }
    public InvoiceBuilder WithDiscountAmount(decimal discount) { _discountAmount = discount; return this; }
    public InvoiceBuilder WithTaxAmount(decimal tax) { _taxAmount = tax; return this; }

    public Invoice Build()
    {
        var errors = new List<string>();

        // Customer
        if (IsBlank(_invoiceId)) errors.Add("InvoiceId is required.");
        if (IsBlank(_customerName)) errors.Add("CustomerName is required.");
        if (IsBlank(_customerEmail)) errors.Add("CustomerEmail is required.");
        else if (!_customerEmail!.Contains('@')) errors.Add("CustomerEmail is not a valid email.");

        // Billing address — this builder has to know every address rule...
        if (IsBlank(_billingStreet)) errors.Add("BillingStreet is required.");
        if (IsBlank(_billingCity)) errors.Add("BillingCity is required.");
        if (IsBlank(_billingZipCode)) errors.Add("BillingZipCode is required.");
        if (IsBlank(_billingCountry)) errors.Add("BillingCountry is required.");

        // ...and repeat them for shipping, but only if any shipping field was given.
        bool hasShipping = !IsBlank(_shippingStreet) || !IsBlank(_shippingCity) || !IsBlank(_shippingState)
                        || !IsBlank(_shippingZipCode) || !IsBlank(_shippingCountry);
        if (hasShipping)
        {
            if (IsBlank(_shippingStreet)) errors.Add("ShippingStreet is required.");
            if (IsBlank(_shippingCity)) errors.Add("ShippingCity is required.");
            if (IsBlank(_shippingZipCode)) errors.Add("ShippingZipCode is required.");
            if (IsBlank(_shippingCountry)) errors.Add("ShippingCountry is required.");
        }

        // Order / payment
        if (_orderDate is null) errors.Add("OrderDate is required.");
        if (_paymentMethod is null) errors.Add("PaymentMethod is required.");
        if (IsBlank(_currency)) errors.Add("Currency is required.");
        else if (_currency!.Trim().Length != 3) errors.Add("Currency must be a 3-letter code (e.g. EGP).");
        if (_subTotal is null) errors.Add("SubTotal is required.");
        else if (_subTotal < 0) errors.Add("SubTotal cannot be negative.");
        if (_discountAmount < 0) errors.Add("DiscountAmount cannot be negative.");
        if (_taxAmount < 0) errors.Add("TaxAmount cannot be negative.");
        if (_subTotal is not null && _discountAmount > _subTotal) errors.Add("DiscountAmount cannot exceed SubTotal.");

        if (errors.Count > 0)
            throw new InvalidOperationException("Cannot build invoice:" + Environment.NewLine + " - " +
                                                string.Join(Environment.NewLine + " - ", errors));

        var billing = new Address(_billingStreet!.Trim(), _billingCity!.Trim(), Clean(_billingState),
                                  _billingZipCode!.Trim(), _billingCountry!.Trim());
        var shipping = hasShipping
            ? new Address(_shippingStreet!.Trim(), _shippingCity!.Trim(), Clean(_shippingState),
                          _shippingZipCode!.Trim(), _shippingCountry!.Trim())
            : billing;

        var order = new OrderDetails(_orderDate!.Value, _paymentMethod!.Value, _currency!.Trim().ToUpperInvariant(),
                                     _subTotal!.Value, _discountAmount, _taxAmount);

        return new Invoice(_invoiceId!.Trim(), _customerName!.Trim(), _customerEmail!.Trim(),
                           Clean(_customerPhone), billing, shipping, order);
    }

    private static bool IsBlank(string? value) => string.IsNullOrWhiteSpace(value);
    private static string? Clean(string? value) => IsBlank(value) ? null : value!.Trim();
}
