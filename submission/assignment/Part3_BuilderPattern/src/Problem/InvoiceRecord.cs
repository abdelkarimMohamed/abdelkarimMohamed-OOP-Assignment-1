using InvoiceBuilding.Models;

namespace InvoiceBuilding.Problem;

/// <summary>
/// Task 3.1 — the ORIGINAL design, kept only to demonstrate the problem.
/// 21 loosely related properties on one flat class, all passed through a single constructor.
/// Do not use this class for new code; see Models/Invoice.cs and the builders instead.
/// </summary>
public class InvoiceRecord
{
    public string InvoiceId { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public string CustomerPhone { get; }

    public string BillingStreet { get; }
    public string BillingCity { get; }
    public string BillingState { get; }
    public string BillingZipCode { get; }
    public string BillingCountry { get; }

    public string ShippingStreet { get; }
    public string ShippingCity { get; }
    public string ShippingState { get; }
    public string ShippingZipCode { get; }
    public string ShippingCountry { get; }

    public DateOnly OrderDate { get; }
    public PaymentMethod PaymentMethod { get; }
    public string Currency { get; }
    public decimal SubTotal { get; }
    public decimal DiscountAmount { get; }
    public decimal TaxAmount { get; }
    public decimal TotalAmount { get; }

    public InvoiceRecord(
        string invoiceId, string customerName, string customerEmail, string customerPhone,
        string billingStreet, string billingCity, string billingState, string billingZipCode, string billingCountry,
        string shippingStreet, string shippingCity, string shippingState, string shippingZipCode, string shippingCountry,
        DateOnly orderDate, PaymentMethod paymentMethod, string currency,
        decimal subTotal, decimal discountAmount, decimal taxAmount, decimal totalAmount)
    {
        InvoiceId = invoiceId;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        CustomerPhone = customerPhone;

        BillingStreet = billingStreet;
        BillingCity = billingCity;
        BillingState = billingState;
        BillingZipCode = billingZipCode;
        BillingCountry = billingCountry;

        ShippingStreet = shippingStreet;
        ShippingCity = shippingCity;
        ShippingState = shippingState;
        ShippingZipCode = shippingZipCode;
        ShippingCountry = shippingCountry;

        OrderDate = orderDate;
        PaymentMethod = paymentMethod;
        Currency = currency;
        SubTotal = subTotal;
        DiscountAmount = discountAmount;
        TaxAmount = taxAmount;
        TotalAmount = totalAmount;
    }
}
