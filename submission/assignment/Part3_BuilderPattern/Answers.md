# Part 3 — Builder Pattern: Written Answers

The class used in this part is an invoice record with 21 properties, in two conceptual families:

| Family | Properties |
|---|---|
| Customer / identity | `InvoiceId`, `CustomerName`, `CustomerEmail`, `CustomerPhone` |
| Address (× 2: billing and shipping) | `Street`, `City`, `State`, `ZipCode`, `Country` |
| Order / payment | `OrderDate`, `PaymentMethod`, `Currency`, `SubTotal`, `DiscountAmount`, `TaxAmount`, `TotalAmount` |

The original flat version with a single 21-parameter constructor is kept in
`src/Problem/InvoiceRecord.cs`, only to demonstrate the problem.

---

## Task 3.1

### Question 1 — Why is a single 20-parameter constructor a problem in practice?

**Call-site readability.** A call such as

```csharp
new InvoiceRecord("INV-1001", "Mona Ali", "mona@example.com", "01000000001",
    "12 Tahrir St", "Cairo", "Cairo Governorate", "11511", "Egypt",
    "12 Tahrir St", "Cairo", "Cairo Governorate", "11511", "Egypt",
    orderDate, PaymentMethod.CreditCard, "EGP", 1000m, 126m, 100m, 1026m);
```

says nothing about what each value means. To read or review it, you have to open the
constructor and count positions. A reviewer cannot tell whether `126m` is the discount or the tax.

**Values passed in the wrong order.** Fourteen of the parameters are `string` and four are
`decimal`. The compiler only checks types, so any two neighbours of the same type can be swapped
and the code still compiles. In the demo, `DiscountAmount` and `TaxAmount` are swapped by mistake:
the program runs, the invoice exists, and its numbers are wrong. The same can happen between
`BillingCity` and `BillingState`, or between a whole billing block and a shipping block.
These bugs are silent — they surface later as wrong totals or parcels sent to the wrong address.

**Optional values have to be passed anyway.** `CustomerPhone`, `State`, a separate shipping
address, discount and tax are all optional in real life, but the constructor forces every caller
to supply something — usually `null`, `""` or `0` — and every caller must remember the correct
"empty" value for each position.

**Consistency is not enforced.** `TotalAmount` is passed in by hand, so nothing guarantees that
it equals `SubTotal - DiscountAmount + TaxAmount`. The demo builds an invoice whose total is 1026
while its own amounts add up to 974, and nothing detects it.

**Adding one more property breaks everyone.** The day someone adds, for example, `ShippingMethod`:
- every existing call site stops compiling and must be edited, even though most don't care about it; or
- a second overloaded constructor is added, then a third — the "telescoping constructor" problem,
  where no one remembers which overload to use; or
- the new parameter is given a default value, which only works at the end of the list and makes the
  positional ordering even harder to follow.

**Validation has no good place to live.** One constructor must validate all 21 values and report
problems about addresses, emails and money in one place, with no way to report several problems
together in a clear way.

### Question 2 — Is this purely a "constructor is too long" problem?

No. The long constructor is a **symptom**; the deeper issue is that the class itself has no
internal structure.

- **Missing concepts.** "Address" is clearly a concept — the same five fields appear twice with a
  `Billing`/`Shipping` prefix. When a naming prefix repeats, it usually means a class is missing.
  The same is true for the order/payment fields, which belong together and have their own rules.
- **Low cohesion / too many responsibilities.** One class holds customer identity, two postal
  addresses and money calculations. These change for different reasons (address format rules,
  tax rules, customer data) but all live in the same place.
- **Duplicated rules.** Without an `Address` type, "an address needs a street, city, zip and
  country" has to be written once for billing and again for shipping — and kept in sync forever.
- **No place for invariants.** Rules like "discount cannot exceed the subtotal" or "the total is
  always computed" belong to the order data, not to the invoice as a whole.

So even with a builder, a flat 21-property class would still be hard to work with. The better fix is
to model the groups as their own types — `Address` (used twice) and `OrderDetails` — and let the
`Invoice` be composed of them. That is exactly what Task 3.3 does:

```text
Invoice
 ├── InvoiceId, CustomerName, CustomerEmail, CustomerPhone
 ├── BillingAddress  : Address      (Street, City, State, ZipCode, Country)
 ├── ShippingAddress : Address      (same type, reused)
 └── Order           : OrderDetails (OrderDate, PaymentMethod, Currency,
                                     SubTotal, DiscountAmount, TaxAmount, TotalAmount = computed)
```

---

## Task 3.2 — Design decisions for the single builder

| Property | Mandatory? | Reason |
|---|---|---|
| `InvoiceId`, `CustomerName`, `CustomerEmail` | Yes | An invoice cannot be issued or sent without them |
| `CustomerPhone` | No | Useful but not required to bill someone |
| Billing `Street`, `City`, `ZipCode`, `Country` | Yes | Legally needed on an invoice |
| Billing / shipping `State` | No | Many countries do not use states |
| Shipping address | No | Defaults to the billing address. If **any** shipping field is given, the whole shipping address becomes mandatory, to avoid half-addresses |
| `OrderDate`, `PaymentMethod`, `Currency`, `SubTotal` | Yes | Needed to calculate and record the payment |
| `DiscountAmount`, `TaxAmount` | No | Default to 0 |
| `TotalAmount` | Not settable | Always computed as `SubTotal - DiscountAmount + TaxAmount` |

`Build()` checks everything and throws one `InvalidOperationException` that lists **every** missing
or invalid field at once, so the caller gets a clear, immediate error instead of an incomplete object.
The `Invoice` constructor is `internal`, so builders are the only way to create one.

---

## Task 3.3 — Why is the composed version better than the single big builder?

### Single responsibility

| Builder | Owns | Does not know about |
|---|---|---|
| `AddressBuilder` | Street, city, state, zip, country, and the rule "an address is complete" | Customers, money, invoices |
| `OrderBuilder` | Date, payment method, currency, amounts, and the money rules | Addresses, customers |
| `ComposedInvoiceBuilder` | Invoice id and customer; putting the parts together | Any address or money rule |

In Task 3.2, one class had six reasons to change (customer rules, billing rules, shipping rules,
money rules, defaults, final assembly). In Task 3.3, each builder has one.

### Independent validation

`AddressBuilder.Build()` guarantees a complete address on its own: it checks street, city, zip and
country and reports exactly what is missing (e.g. *"Billing address is incomplete. Missing: City,
ZipCode."*). The parent builder never sees those fields — it only receives a finished, valid
`Address` object or an exception. The same holds for `OrderBuilder` and the money rules
(non-negative amounts, discount not greater than subtotal, 3-letter currency).

Because each part is validated as soon as it is configured, an error points directly at the part
that is wrong, instead of a long list mixing addresses and amounts.

### Reuse

The exact same `AddressBuilder` class is used for the billing and the shipping address. Without it,
Task 3.2 had to duplicate:
- **5 fields × 2** (`_billingStreet` … `_shippingCountry`),
- **5 `With…` methods × 2** (`WithBillingStreet` … `WithShippingCountry`),
- **4 validation checks × 2**, plus the trimming and the `new Address(...)` call.

That is 10 methods and 10 fields that exist only because of a prefix. If a rule changes (for
example, validating the zip code format), in Task 3.2 it must be changed in two places and it is
easy to forget one; in Task 3.3 it changes once. A third address (for example a "return address")
would cost nothing in the composed version, and another 5 fields + 5 methods + 4 checks in the
single builder. `AddressBuilder` can also be reused outside invoices entirely (customer profiles,
warehouses).

### Readability at the call site

Task 3.2 — a flat list of 18 calls where the grouping exists only in the method-name prefixes:

```csharp
new InvoiceBuilder()
    .WithInvoiceId("INV-1001")
    .WithCustomerName("Mona Ali")
    .WithCustomerEmail("mona@example.com")
    .WithBillingStreet("12 Tahrir St")
    .WithBillingCity("Cairo")
    .WithBillingZipCode("11511")
    .WithBillingCountry("Egypt")
    .WithShippingStreet("5 Pyramids Rd")
    .WithShippingCity("Giza")
    // ...
    .WithSubTotal(1000m)
    .WithDiscountAmount(100m)
    .WithTaxAmount(126m)
    .Build();
```

Task 3.3 — the code has the same shape as the data. Each block reads as one unit, and it is
obvious which fields belong to billing, shipping or the order:

```csharp
new ComposedInvoiceBuilder()
    .WithInvoiceId("INV-1001")
    .ForCustomer("Mona Ali", "mona@example.com", phone: "01000000001")
    .WithBillingAddress(a => a
        .WithStreet("12 Tahrir St").WithCity("Cairo").WithZipCode("11511").WithCountry("Egypt"))
    .WithShippingAddress(a => a
        .WithStreet("5 Pyramids Rd").WithCity("Giza").WithZipCode("12511").WithCountry("Egypt"))
    .WithOrder(o => o
        .WithOrderDate(orderDate).WithPaymentMethod(PaymentMethod.CreditCard)
        .WithCurrency("EGP").WithSubTotal(1000m).WithDiscount(100m).WithTax(126m))
    .Build();
```

Both produce an identical `Invoice` (the demo checks this), but the composed version is shorter,
grouped, and much harder to get wrong — you cannot accidentally set a shipping city while
configuring the billing address, because inside that block there is only one kind of address.

### Summary

| Aspect | Single builder (3.2) | Composed builders (3.3) |
|---|---|---|
| Responsibilities per class | Everything | One group each |
| Address rules written | Twice | Once |
| Methods for addresses | 10 | 5 (reused) |
| Where an error points | One mixed list | The exact part that is wrong |
| Adding a third address | +5 fields, +5 methods, +4 checks | One new `With…Address` method |
| Call site | Flat, grouping only by prefix | Mirrors the structure of the data |
