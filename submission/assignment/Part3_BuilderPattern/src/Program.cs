using InvoiceBuilding.Builders.Composed;
using InvoiceBuilding.Builders.Single;
using InvoiceBuilding.Models;
using InvoiceBuilding.Problem;

var orderDate = new DateOnly(2026, 9, 25);

// =====================================================================
// Task 3.1 — the problem: a 21-parameter constructor
// =====================================================================
Section("Task 3.1 - 21-parameter constructor");

// Can you tell which value is which? The compiler can't either.
// Discount (100) and tax (126) are swapped by mistake, and the total is typed by hand.
var record = new InvoiceRecord(
    "INV-1001", "Mona Ali", "mona@example.com", "01000000001",
    "12 Tahrir St", "Cairo", "Cairo Governorate", "11511", "Egypt",
    "12 Tahrir St", "Cairo", "Cairo Governorate", "11511", "Egypt",
    orderDate, PaymentMethod.CreditCard, "EGP",
    1000m, 126m, 100m, 1026m);

Console.WriteLine("  It compiles and runs, but the values are wrong:");
Console.WriteLine($"  Discount={record.DiscountAmount}, Tax={record.TaxAmount} (swapped by mistake)");
Console.WriteLine($"  TotalAmount={record.TotalAmount} (typed by hand), but SubTotal - Discount + Tax = " +
                  $"{record.SubTotal - record.DiscountAmount + record.TaxAmount}");
Console.WriteLine("  The object is inconsistent and nothing detected it.");

// =====================================================================
// Task 3.2 — one big fluent builder
// =====================================================================
Section("Task 3.2 - single InvoiceBuilder");

var invoiceA = new InvoiceBuilder()
    .WithInvoiceId("INV-1001")
    .WithCustomerName("Mona Ali")
    .WithCustomerEmail("mona@example.com")
    .WithCustomerPhone("01000000001")
    .WithBillingStreet("12 Tahrir St")
    .WithBillingCity("Cairo")
    .WithBillingZipCode("11511")
    .WithBillingCountry("Egypt")
    .WithShippingStreet("5 Pyramids Rd")
    .WithShippingCity("Giza")
    .WithShippingZipCode("12511")
    .WithShippingCountry("Egypt")
    .WithOrderDate(orderDate)
    .WithPaymentMethod(PaymentMethod.CreditCard)
    .WithCurrency("egp")
    .WithSubTotal(1000m)
    .WithDiscountAmount(100m)
    .WithTaxAmount(126m)
    .Build();
Print(invoiceA);

Try("Forget several mandatory fields", () =>
    new InvoiceBuilder()
        .WithInvoiceId("INV-1002")
        .WithCustomerName("Omar Hassan")
        .WithBillingStreet("1 Corniche")
        .WithOrderDate(orderDate)
        .WithSubTotal(500m)
        .Build());

Try("Give only part of a shipping address", () =>
    new InvoiceBuilder()
        .WithInvoiceId("INV-1003").WithCustomerName("Sara Nabil").WithCustomerEmail("sara@example.com")
        .WithBillingStreet("3 Nile St").WithBillingCity("Cairo").WithBillingZipCode("11311").WithBillingCountry("Egypt")
        .WithShippingCity("Alexandria")
        .WithOrderDate(orderDate).WithPaymentMethod(PaymentMethod.Cash).WithCurrency("EGP").WithSubTotal(200m)
        .Build());

// =====================================================================
// Task 3.3 — composed builders: AddressBuilder (reused) + OrderBuilder
// =====================================================================
Section("Task 3.3 - ComposedInvoiceBuilder (AddressBuilder + OrderBuilder)");

var invoiceB = new ComposedInvoiceBuilder()
    .WithInvoiceId("INV-1001")
    .ForCustomer("Mona Ali", "mona@example.com", phone: "01000000001")
    .WithBillingAddress(a => a
        .WithStreet("12 Tahrir St")
        .WithCity("Cairo")
        .WithZipCode("11511")
        .WithCountry("Egypt"))
    .WithShippingAddress(a => a
        .WithStreet("5 Pyramids Rd")
        .WithCity("Giza")
        .WithZipCode("12511")
        .WithCountry("Egypt"))
    .WithOrder(o => o
        .WithOrderDate(orderDate)
        .WithPaymentMethod(PaymentMethod.CreditCard)
        .WithCurrency("EGP")
        .WithSubTotal(1000m)
        .WithDiscount(100m)
        .WithTax(126m))
    .Build();
Print(invoiceB);

Console.WriteLine($"  Same result as Task 3.2? {invoiceA.TotalAmount == invoiceB.TotalAmount && invoiceA.ShippingAddress.ToString() == invoiceB.ShippingAddress.ToString()}");

Section("Task 3.3 - shipping address omitted (defaults to billing)");
var invoiceC = new ComposedInvoiceBuilder()
    .WithInvoiceId("INV-1004")
    .ForCustomer("Omar Hassan", "omar@example.com")
    .WithBillingAddress(a => a.WithStreet("1 Corniche").WithCity("Alexandria").WithZipCode("21500").WithCountry("Egypt"))
    .WithOrder(o => o.WithOrderDate(orderDate).WithPaymentMethod(PaymentMethod.Wallet).WithCurrency("EGP").WithSubTotal(250m))
    .Build();
Print(invoiceC);

Section("Task 3.3 - each small builder validates itself");
Try("Incomplete billing address (AddressBuilder)", () =>
    new ComposedInvoiceBuilder()
        .WithBillingAddress(a => a.WithStreet("1 Corniche").WithCountry("Egypt")));

Try("Incomplete shipping address (same AddressBuilder)", () =>
    new ComposedInvoiceBuilder()
        .WithShippingAddress(a => a.WithCity("Giza")));

Try("Discount larger than subtotal (OrderBuilder)", () =>
    new ComposedInvoiceBuilder()
        .WithOrder(o => o.WithOrderDate(orderDate).WithPaymentMethod(PaymentMethod.Cash)
                         .WithCurrency("EGP").WithSubTotal(100m).WithDiscount(150m)));

Try("Missing invoice-level fields (parent builder)", () =>
    new ComposedInvoiceBuilder()
        .WithInvoiceId("INV-1005")
        .Build());

Console.WriteLine();
Console.WriteLine("Demo finished.");

// ---------- helpers ----------

static void Section(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}

static void Print(Invoice invoice)
{
    var o = invoice.Order;
    Console.WriteLine($"  Invoice {invoice.InvoiceId} | {invoice.CustomerName} <{invoice.CustomerEmail}> | phone: {invoice.CustomerPhone ?? "-"}");
    Console.WriteLine($"    Billing : {invoice.BillingAddress}");
    Console.WriteLine($"    Shipping: {invoice.ShippingAddress}");
    Console.WriteLine($"    Order   : {o.OrderDate:yyyy-MM-dd} | {o.PaymentMethod} | {o.Currency}");
    Console.WriteLine($"    Amounts : subtotal {o.SubTotal:F2} - discount {o.DiscountAmount:F2} + tax {o.TaxAmount:F2} = TOTAL {o.TotalAmount:F2}");
}

static void Try(string description, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [NOT BLOCKED!] {description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [BLOCKED] {description}");
        foreach (var line in ex.Message.Split(Environment.NewLine))
            Console.WriteLine($"            {line}");
    }
}
